// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Catalog;

public sealed record DocumentInput(string Title, string? Publisher, string? Version, DateOnly? PublicationDate, string? Source, DocumentUsageState UsageState, string? UsageStateReason, string? Notes);
[RequiresPermission("Documents.Manage")] public sealed record CreateDocumentCommand(DocumentInput Input) : IRequest<Result<long>>;
[RequiresPermission("Documents.Manage")] public sealed record UpdateDocumentCommand(long DocumentId, DocumentInput Input, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Documents.Manage")] public sealed record CreateCatalogVersionCommand(long DocumentId, string? Notes) : IRequest<Result<long>>;
[RequiresPermission("Documents.Manage")] public sealed record AddDocumentElementCommand(long CatalogVersionId, long? ParentElementId, string? Title, string? Text, string? Notes) : IRequest<Result<long>>;
[RequiresPermission("Documents.Manage")] public sealed record UpdateDocumentElementCommand(long ElementId, string? Title, string? Text, string? Notes) : IRequest<Result>;
[RequiresPermission("Documents.Manage")] public sealed record MoveDocumentElementCommand(long ElementId, long? ParentElementId, int SortOrder) : IRequest<Result>;
public sealed record DeleteDocumentElementPreview(int DescendantElementCount, int QuestionCount, int ScopeAssignmentCount);
[RequiresPermission("Documents.Manage")] public sealed record GetDeleteDocumentElementPreviewQuery(long ElementId) : IRequest<Result<DeleteDocumentElementPreview>>;
[RequiresPermission("Documents.Manage")] public sealed record DeleteDocumentElementCommand(long ElementId, bool Confirmed) : IRequest<Result>;
[RequiresPermission("Documents.Manage")] public sealed record AddQuestionCommand(long ElementId, string Text, string? VerificationHint, string? EvidenceHint, string? Notes, IReadOnlyCollection<long> ScopeTypeIds) : IRequest<Result<long>>;
[RequiresPermission("Documents.Manage")] public sealed record UpdateQuestionCommand(long QuestionId, string Text, string? VerificationHint, string? EvidenceHint, string? Notes, IReadOnlyCollection<long> ScopeTypeIds) : IRequest<Result>;
[RequiresPermission("Documents.Manage")] public sealed record SetDocumentElementWeightCommand(long ElementId, int Weight) : IRequest<Result>;
[RequiresPermission("Documents.Manage")] public sealed record SetCatalogReadyCommand(long CatalogVersionId, bool Confirmed) : IRequest<Result>;
[RequiresPermission("Documents.Manage")] public sealed record SetCatalogDraftCommand(long CatalogVersionId, bool Confirmed) : IRequest<Result>;
[RequiresPermission("Documents.Manage")] public sealed record CopyCatalogVersionCommand(long CatalogVersionId) : IRequest<Result<long>>;

public sealed class CatalogCommandHandler(IAuditariumDbContext db, ICurrentActor actor) :
    IRequestHandler<CreateDocumentCommand, Result<long>>, IRequestHandler<UpdateDocumentCommand, Result>, IRequestHandler<CreateCatalogVersionCommand, Result<long>>,
    IRequestHandler<AddDocumentElementCommand, Result<long>>, IRequestHandler<UpdateDocumentElementCommand, Result>, IRequestHandler<MoveDocumentElementCommand, Result>,
    IRequestHandler<GetDeleteDocumentElementPreviewQuery, Result<DeleteDocumentElementPreview>>, IRequestHandler<DeleteDocumentElementCommand, Result>, IRequestHandler<AddQuestionCommand, Result<long>>, IRequestHandler<UpdateQuestionCommand, Result>,
    IRequestHandler<SetDocumentElementWeightCommand, Result>, IRequestHandler<SetCatalogReadyCommand, Result>, IRequestHandler<SetCatalogDraftCommand, Result>, IRequestHandler<CopyCatalogVersionCommand, Result<long>>
{
    public async ValueTask<Result<long>> Handle(CreateDocumentCommand message, CancellationToken ct)
    {
        var error = ValidateDocument(message.Input); if (error is not null) return Failure<long>(error);
        var document = new Document { Title = message.Input.Title.Trim(), Publisher = Null(message.Input.Publisher), Version = Null(message.Input.Version), PublicationDate = message.Input.PublicationDate, Source = Null(message.Input.Source), UsageState = message.Input.UsageState, UsageStateReason = Null(message.Input.UsageStateReason), Notes = Null(message.Input.Notes) };
        db.Documents.Add(document); await db.SaveChangesAsync(ct); return Result<long>.Success(document.DocumentId);
    }

    public async ValueTask<Result> Handle(UpdateDocumentCommand message, CancellationToken ct)
    {
        var error = ValidateDocument(message.Input); if (error is not null) return Failure(error);
        var document = await db.Documents.SingleOrDefaultAsync(x => x.DocumentId == message.DocumentId, ct); if (document is null) return NotFound();
        if (document.ConcurrencyVersion != message.ConcurrencyVersion) return Conflict();
        document.Title = message.Input.Title.Trim(); document.Publisher = Null(message.Input.Publisher); document.Version = Null(message.Input.Version); document.PublicationDate = message.Input.PublicationDate; document.Source = Null(message.Input.Source); document.UsageState = message.Input.UsageState; document.UsageStateReason = Null(message.Input.UsageStateReason); document.Notes = Null(message.Input.Notes);
        await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result<long>> Handle(CreateCatalogVersionCommand message, CancellationToken ct)
    {
        if (!await db.Documents.AnyAsync(x => x.DocumentId == message.DocumentId, ct)) return Failure<long>("DOCUMENT.NOT_FOUND", ErrorType.NotFound);
        var next = (await db.CatalogVersions.Where(x => x.DocumentId == message.DocumentId).Select(x => (int?)x.VersionNumber).MaxAsync(ct) ?? 0) + 1;
        var version = new CatalogVersion { DocumentId = message.DocumentId, VersionNumber = next, CreatedAt = DateTimeOffset.UtcNow, CreatedBy = actor.UserId ?? 0, Notes = Null(message.Notes) };
        db.CatalogVersions.Add(version); await db.SaveChangesAsync(ct); return Result<long>.Success(version.CatalogVersionId);
    }

    public async ValueTask<Result<long>> Handle(AddDocumentElementCommand message, CancellationToken ct)
    {
        var catalog = await EditableCatalogAsync(message.CatalogVersionId, ct); if (catalog.Error is not null) return Failure<long>(catalog.Error);
        if (Empty(message.Title) && Empty(message.Text)) return Failure<long>("CATALOG.ELEMENT_CONTENT_REQUIRED", ErrorType.Validation);
        if (message.ParentElementId is { } parent && !await db.DocumentElements.AnyAsync(x => x.ElementId == parent && x.CatalogVersionId == message.CatalogVersionId, ct)) return Failure<long>("CATALOG.PARENT_INVALID", ErrorType.Validation);
        var sort = (await db.DocumentElements.Where(x => x.CatalogVersionId == message.CatalogVersionId && x.ParentElementId == message.ParentElementId).Select(x => (int?)x.SortOrder).MaxAsync(ct) ?? -1) + 1;
        var element = new DocumentElement { CatalogVersionId = message.CatalogVersionId, ParentElementId = message.ParentElementId, Title = Null(message.Title), Text = Null(message.Text), Notes = Null(message.Notes), SortOrder = sort };
        db.DocumentElements.Add(element); catalog.Value!.DraftRevision++; await db.SaveChangesAsync(ct); return Result<long>.Success(element.ElementId);
    }

    public async ValueTask<Result> Handle(UpdateDocumentElementCommand message, CancellationToken ct)
    {
        var element = await db.DocumentElements.SingleOrDefaultAsync(x => x.ElementId == message.ElementId, ct); if (element is null) return NotFound();
        var catalog = await EditableCatalogAsync(element.CatalogVersionId, ct); if (catalog.Error is not null) return Failure(catalog.Error);
        if (Empty(message.Title) && Empty(message.Text)) return Failure("CATALOG.ELEMENT_CONTENT_REQUIRED", ErrorType.Validation);
        if (!string.IsNullOrWhiteSpace(message.Text) || !await db.Questions.AnyAsync(x => x.ElementId == element.ElementId, ct)) { element.Title = Null(message.Title); element.Text = Null(message.Text); element.Notes = Null(message.Notes); catalog.Value!.DraftRevision++; await db.SaveChangesAsync(ct); return Result.Success(); }
        return Failure("CATALOG.QUESTIONED_ELEMENT_TEXT_REQUIRED", ErrorType.Validation);
    }

    public async ValueTask<Result> Handle(MoveDocumentElementCommand message, CancellationToken ct)
    {
        var element = await db.DocumentElements.SingleOrDefaultAsync(x => x.ElementId == message.ElementId, ct); if (element is null) return NotFound();
        var catalog = await EditableCatalogAsync(element.CatalogVersionId, ct); if (catalog.Error is not null) return Failure(catalog.Error);
        if (message.SortOrder < 0) return Failure("CATALOG.SORT_ORDER_INVALID", ErrorType.Validation);
        if (message.ParentElementId is { } parent && (!await db.DocumentElements.AnyAsync(x => x.ElementId == parent && x.CatalogVersionId == element.CatalogVersionId, ct) || await IsDescendantAsync(parent, element.ElementId, ct))) return Failure("CATALOG.PARENT_INVALID", ErrorType.Validation);
        await NormalizeSiblingsAsync(element.CatalogVersionId, element.ParentElementId, element.ElementId, ct); element.ParentElementId = message.ParentElementId;
        var siblings = await db.DocumentElements.Where(x => x.CatalogVersionId == element.CatalogVersionId && x.ParentElementId == message.ParentElementId && x.ElementId != element.ElementId).OrderBy(x => x.SortOrder).ToListAsync(ct); siblings.Insert(Math.Min(message.SortOrder, siblings.Count), element); for (var i = 0; i < siblings.Count; i++) siblings[i].SortOrder = i;
        catalog.Value!.DraftRevision++; await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result<DeleteDocumentElementPreview>> Handle(GetDeleteDocumentElementPreviewQuery message, CancellationToken ct)
    {
        var element = await db.DocumentElements.SingleOrDefaultAsync(x => x.ElementId == message.ElementId, ct); if (element is null) return Failure<DeleteDocumentElementPreview>("CATALOG.ELEMENT_NOT_FOUND", ErrorType.NotFound);
        var catalog = await EditableCatalogAsync(element.CatalogVersionId, ct); if (catalog.Error is not null) return Failure<DeleteDocumentElementPreview>(catalog.Error);
        var ids = await DescendantIdsAsync(element.ElementId, ct); var questions = await db.Questions.Where(x => ids.Contains(x.ElementId)).Select(x => x.QuestionId).ToListAsync(ct); var assignments = await db.QuestionScopeTypes.CountAsync(x => questions.Contains(x.QuestionId), ct);
        return Result<DeleteDocumentElementPreview>.Success(new(ids.Count - 1, questions.Count, assignments));
    }

    public async ValueTask<Result> Handle(DeleteDocumentElementCommand message, CancellationToken ct)
    {
        var element = await db.DocumentElements.SingleOrDefaultAsync(x => x.ElementId == message.ElementId, ct); if (element is null) return NotFound();
        var catalog = await EditableCatalogAsync(element.CatalogVersionId, ct); if (catalog.Error is not null) return Failure(catalog.Error);
        if (!message.Confirmed) return Failure("CATALOG.DELETE_CONFIRMATION_REQUIRED", ErrorType.Validation);
        var subtreeIds = await DescendantIdsAsync(element.ElementId, ct);
        var subtree = await db.DocumentElements.Where(x => subtreeIds.Contains(x.ElementId)).OrderByDescending(x => x.ElementId).ToListAsync(ct);
        db.DocumentElements.RemoveRange(subtree); catalog.Value!.DraftRevision++; await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result<long>> Handle(AddQuestionCommand message, CancellationToken ct)
    {
        var element = await db.DocumentElements.SingleOrDefaultAsync(x => x.ElementId == message.ElementId, ct); if (element is null) return Failure<long>("CATALOG.ELEMENT_NOT_FOUND", ErrorType.NotFound);
        var catalog = await EditableCatalogAsync(element.CatalogVersionId, ct); if (catalog.Error is not null) return Failure<long>(catalog.Error);
        if (Empty(message.Text) || Empty(element.Text)) return Failure<long>("CATALOG.QUESTION_TEXT_REQUIRED", ErrorType.Validation);
        if (!await ScopeTypesValidAsync(message.ScopeTypeIds, ct)) return Failure<long>("CATALOG.SCOPE_TYPE_INVALID", ErrorType.Validation);
        var question = new Question { ElementId = element.ElementId, SortOrder = (await db.Questions.Where(x => x.ElementId == element.ElementId).Select(x => (int?)x.SortOrder).MaxAsync(ct) ?? -1) + 1, Text = message.Text.Trim(), VerificationHint = Null(message.VerificationHint), EvidenceHint = Null(message.EvidenceHint), Notes = Null(message.Notes) };
        db.Questions.Add(question); await db.SaveChangesAsync(ct); foreach (var scopeTypeId in message.ScopeTypeIds.Distinct()) db.QuestionScopeTypes.Add(new QuestionScopeType { QuestionId = question.QuestionId, ScopeTypeId = scopeTypeId }); catalog.Value!.DraftRevision++; await db.SaveChangesAsync(ct); return Result<long>.Success(question.QuestionId);
    }

    public async ValueTask<Result> Handle(UpdateQuestionCommand message, CancellationToken ct)
    {
        var question = await db.Questions.SingleOrDefaultAsync(x => x.QuestionId == message.QuestionId, ct); if (question is null) return NotFound(); var element = await db.DocumentElements.SingleAsync(x => x.ElementId == question.ElementId, ct); var catalog = await EditableCatalogAsync(element.CatalogVersionId, ct); if (catalog.Error is not null) return Failure(catalog.Error);
        if (Empty(message.Text) || !await ScopeTypesValidAsync(message.ScopeTypeIds, ct)) return Failure("CATALOG.QUESTION_INVALID", ErrorType.Validation);
        question.Text = message.Text.Trim(); question.VerificationHint = Null(message.VerificationHint); question.EvidenceHint = Null(message.EvidenceHint); question.Notes = Null(message.Notes); db.QuestionScopeTypes.RemoveRange(db.QuestionScopeTypes.Where(x => x.QuestionId == question.QuestionId)); foreach (var scopeTypeId in message.ScopeTypeIds.Distinct()) db.QuestionScopeTypes.Add(new QuestionScopeType { QuestionId = question.QuestionId, ScopeTypeId = scopeTypeId }); catalog.Value!.DraftRevision++; await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result> Handle(SetDocumentElementWeightCommand message, CancellationToken ct)
    {
        var element = await db.DocumentElements.SingleOrDefaultAsync(x => x.ElementId == message.ElementId, ct); if (element is null) return NotFound(); if (!await db.Questions.AnyAsync(x => x.ElementId == element.ElementId, ct)) return Failure("CATALOG.WEIGHT_REQUIRES_QUESTION", ErrorType.Validation); if (message.Weight is < 1 or > 5) return Failure("CATALOG.WEIGHT_INVALID", ErrorType.Validation);
        var existing = await db.DocumentElementWeights.SingleOrDefaultAsync(x => x.ElementId == element.ElementId, ct); if (message.Weight == 3) { if (existing is not null) db.DocumentElementWeights.Remove(existing); } else if (existing is null) db.DocumentElementWeights.Add(new DocumentElementWeight { ElementId = element.ElementId, Weight = message.Weight }); else existing.Weight = message.Weight;
        await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result> Handle(SetCatalogReadyCommand message, CancellationToken ct)
    {
        if (!message.Confirmed) return Failure("CATALOG.READY_CONFIRMATION_REQUIRED", ErrorType.Validation); var catalog = await db.CatalogVersions.SingleOrDefaultAsync(x => x.CatalogVersionId == message.CatalogVersionId, ct); if (catalog is null) return NotFound(); if (catalog.CatalogState != CatalogState.Draft) return Failure("CATALOG.NOT_DRAFT", ErrorType.Conflict);
        var error = await ValidateReadyAsync(catalog.CatalogVersionId, ct); if (error is not null) return Failure(error, ErrorType.Validation); catalog.CatalogState = CatalogState.Ready; await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result> Handle(SetCatalogDraftCommand message, CancellationToken ct)
    {
        if (!message.Confirmed) return Failure("CATALOG.DRAFT_CONFIRMATION_REQUIRED", ErrorType.Validation);
        var catalog = await db.CatalogVersions.SingleOrDefaultAsync(x => x.CatalogVersionId == message.CatalogVersionId, ct); if (catalog is null) return NotFound();
        if (catalog.CatalogState != CatalogState.Ready) return Failure("CATALOG.NOT_READY", ErrorType.Conflict);
        if (await db.Audits.AnyAsync(x => x.CatalogVersionId == catalog.CatalogVersionId, ct)) return Failure("CATALOG.USED_VERSION_CREATE_DRAFT_COPY", ErrorType.Conflict);
        catalog.CatalogState = CatalogState.Draft; catalog.DraftRevision++; await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result<long>> Handle(CopyCatalogVersionCommand message, CancellationToken ct)
    {
        var source = await db.CatalogVersions.SingleOrDefaultAsync(x => x.CatalogVersionId == message.CatalogVersionId, ct); if (source is null) return Failure<long>("CATALOG.NOT_FOUND", ErrorType.NotFound); var next = (await db.CatalogVersions.Where(x => x.DocumentId == source.DocumentId).Select(x => (int?)x.VersionNumber).MaxAsync(ct) ?? 0) + 1; var copy = new CatalogVersion { DocumentId = source.DocumentId, VersionNumber = next, CreatedAt = DateTimeOffset.UtcNow, CreatedBy = actor.UserId ?? 0, Notes = source.Notes, SourceFileId = source.SourceFileId }; db.CatalogVersions.Add(copy); await db.SaveChangesAsync(ct);
        var elements = await db.DocumentElements.Where(x => x.CatalogVersionId == source.CatalogVersionId).OrderBy(x => x.ElementId).ToListAsync(ct); var ids = new Dictionary<long, long>(); foreach (var item in elements) { var added = new DocumentElement { CatalogVersionId = copy.CatalogVersionId, ParentElementId = item.ParentElementId, Title = item.Title, Text = item.Text, SortOrder = item.SortOrder, Notes = item.Notes }; db.DocumentElements.Add(added); await db.SaveChangesAsync(ct); ids[item.ElementId] = added.ElementId; }
        foreach (var item in elements.Where(x => x.ParentElementId is not null)) db.DocumentElements.Single(x => x.ElementId == ids[item.ElementId]).ParentElementId = ids[item.ParentElementId!.Value];
        var questions = await db.Questions.Where(x => elements.Select(e => e.ElementId).Contains(x.ElementId)).ToListAsync(ct); var questionIds = new Dictionary<long, long>(); foreach (var item in questions) { var added = new Question { ElementId = ids[item.ElementId], SortOrder = item.SortOrder, Text = item.Text, VerificationHint = item.VerificationHint, EvidenceHint = item.EvidenceHint, Notes = item.Notes }; db.Questions.Add(added); await db.SaveChangesAsync(ct); questionIds[item.QuestionId] = added.QuestionId; }
        var scopes = await db.QuestionScopeTypes.Where(x => questionIds.Keys.Contains(x.QuestionId)).ToListAsync(ct); foreach (var item in scopes) db.QuestionScopeTypes.Add(new QuestionScopeType { QuestionId = questionIds[item.QuestionId], ScopeTypeId = item.ScopeTypeId }); var weights = await db.DocumentElementWeights.Where(x => ids.Keys.Contains(x.ElementId)).ToListAsync(ct); foreach (var item in weights) db.DocumentElementWeights.Add(new DocumentElementWeight { ElementId = ids[item.ElementId], Weight = item.Weight }); await db.SaveChangesAsync(ct); return Result<long>.Success(copy.CatalogVersionId);
    }

    private async Task<(CatalogVersion? Value, string? Error)> EditableCatalogAsync(long id, CancellationToken ct) { var catalog = await db.CatalogVersions.SingleOrDefaultAsync(x => x.CatalogVersionId == id, ct); return catalog is null ? (null, "CATALOG.NOT_FOUND") : catalog.CatalogState != CatalogState.Draft ? (null, "CATALOG.NOT_DRAFT") : (catalog, null); }
    private async Task<bool> IsDescendantAsync(long candidate, long ancestor, CancellationToken ct) { var current = await db.DocumentElements.SingleAsync(x => x.ElementId == candidate, ct); while (current.ParentElementId is { } parent) { if (parent == ancestor) return true; current = await db.DocumentElements.SingleAsync(x => x.ElementId == parent, ct); } return false; }
    private async Task<List<long>> DescendantIdsAsync(long rootId, CancellationToken ct) { var result = new List<long> { rootId }; for (var index = 0; index < result.Count; index++) result.AddRange(await db.DocumentElements.Where(x => x.ParentElementId == result[index]).Select(x => x.ElementId).ToListAsync(ct)); return result; }
    private async Task NormalizeSiblingsAsync(long catalogId, long? parentId, long excludedId, CancellationToken ct) { var siblings = await db.DocumentElements.Where(x => x.CatalogVersionId == catalogId && x.ParentElementId == parentId && x.ElementId != excludedId).OrderBy(x => x.SortOrder).ToListAsync(ct); for (var i = 0; i < siblings.Count; i++) siblings[i].SortOrder = i; }
    private async Task<bool> ScopeTypesValidAsync(IReadOnlyCollection<long> ids, CancellationToken ct) => ids.Count == ids.Distinct().Count() && await db.ScopeTypes.CountAsync(x => ids.Contains(x.ScopeTypeId), ct) == ids.Count;
    private async Task<string?> ValidateReadyAsync(long catalogId, CancellationToken ct) { var elements = await db.DocumentElements.Where(x => x.CatalogVersionId == catalogId).ToListAsync(ct); foreach (var element in elements) { if (element.ParentElementId is { } parent && !elements.Any(x => x.ElementId == parent)) return "CATALOG.PARENT_INVALID"; if (await db.Questions.AnyAsync(x => x.ElementId == element.ElementId, ct) && Empty(element.Text)) return "CATALOG.QUESTIONED_ELEMENT_TEXT_REQUIRED"; } if (await db.Questions.Where(x => elements.Select(e => e.ElementId).Contains(x.ElementId)).AnyAsync(x => !db.QuestionScopeTypes.Any(s => s.QuestionId == x.QuestionId), ct)) return "CATALOG.QUESTION_SCOPE_REQUIRED"; return null; }
    private static AppError? ValidateDocument(DocumentInput x) => Empty(x.Title) ? new("DOCUMENT.TITLE_REQUIRED", ErrorType.Validation) : x.UsageState == DocumentUsageState.Deprecated && Empty(x.UsageStateReason) ? new("DOCUMENT.DEPRECATION_REASON_REQUIRED", ErrorType.Validation) : null;
    private static bool Empty(string? value) => string.IsNullOrWhiteSpace(value); private static string? Null(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim(); private static Result Failure(AppError error) => Result.Failure(error); private static Result<T> Failure<T>(AppError error) => Result<T>.Failure(error); private static Result Failure(string code) => Failure(code, ErrorType.Validation); private static Result<T> Failure<T>(string code) => Failure<T>(code, ErrorType.Validation); private static Result Failure(string code, ErrorType type) => Result.Failure(new AppError(code, type)); private static Result<T> Failure<T>(string code, ErrorType type) => Result<T>.Failure(new AppError(code, type)); private static Result NotFound() => Failure("CATALOG.NOT_FOUND", ErrorType.NotFound); private static Result Conflict() => Failure("CATALOG.CONCURRENCY_CONFLICT", ErrorType.Conflict);
}
