// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Catalog;

public sealed record DocumentListItem(long DocumentId, string Title, string? Publisher, string? Version, DocumentUsageState UsageState, long ConcurrencyVersion);
public sealed record DocumentDetails(long DocumentId, string Title, string? Publisher, string? Version, DateOnly? PublicationDate, string? Source, DocumentUsageState UsageState, string? UsageStateReason, string? Notes, long ConcurrencyVersion);
public sealed record CatalogVersionListItem(long CatalogVersionId, int VersionNumber, CatalogState CatalogState, int DraftRevision, DateTimeOffset CreatedAt, long CreatedBy, bool HasSourceFile, long ConcurrencyVersion);
public sealed record CatalogVersionDetails(long CatalogVersionId, long DocumentId, int VersionNumber, CatalogState CatalogState, int DraftRevision, string? Notes, bool HasSourceFile, long ConcurrencyVersion);
public sealed record ScopeTypeItem(long ScopeTypeId, string Key, string Name, string Description);
public sealed record QuestionEditorItem(long QuestionId, int SortOrder, string Text, string? VerificationHint, string? EvidenceHint, string? Notes, IReadOnlyList<long> ScopeTypeIds);
public sealed record DocumentElementEditorItem(long ElementId, long? ParentElementId, int SortOrder, string? Title, string? Text, string? Notes, int Weight, IReadOnlyList<QuestionEditorItem> Questions);
public sealed record CatalogEditorDetails(CatalogVersionDetails Catalog, IReadOnlyList<DocumentElementEditorItem> Elements, IReadOnlyList<ScopeTypeItem> ScopeTypes);
public sealed record DocumentPage(IReadOnlyList<DocumentListItem> Items, long TotalCount);

[RequiresPermission("Documents.Manage")] public sealed record ListDocumentsQuery(string? Search, int Skip, int Take, string Sort, bool Descending) : IRequest<Result<DocumentPage>>;
[RequiresPermission("Documents.Manage")] public sealed record GetDocumentQuery(long DocumentId) : IRequest<Result<DocumentDetails>>;
[RequiresPermission("Documents.Manage")] public sealed record ListCatalogVersionsQuery(long DocumentId) : IRequest<Result<IReadOnlyList<CatalogVersionListItem>>>;
[RequiresPermission("Documents.Manage")] public sealed record GetCatalogVersionQuery(long CatalogVersionId) : IRequest<Result<CatalogVersionDetails>>;
[RequiresPermission("Documents.Manage")] public sealed record GetCatalogEditorQuery(long CatalogVersionId) : IRequest<Result<CatalogEditorDetails>>;

public sealed class CatalogQueryHandler(IAuditariumDbContext db) :
    IRequestHandler<ListDocumentsQuery, Result<DocumentPage>>, IRequestHandler<GetDocumentQuery, Result<DocumentDetails>>,
    IRequestHandler<ListCatalogVersionsQuery, Result<IReadOnlyList<CatalogVersionListItem>>>, IRequestHandler<GetCatalogVersionQuery, Result<CatalogVersionDetails>>, IRequestHandler<GetCatalogEditorQuery, Result<CatalogEditorDetails>>
{
    public async ValueTask<Result<DocumentPage>> Handle(ListDocumentsQuery query, CancellationToken ct)
    {
        if (query.Skip < 0 || query.Take is < 1 or > 200 || query.Sort is not ("title" or "publisher" or "version" or "usageState"))
            return Result<DocumentPage>.Failure(new AppError("DOCUMENT.LIST_ARGUMENT_INVALID", ErrorType.Validation));
        var documents = db.Documents.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            documents = documents.Where(x => x.Title.Contains(search) || (x.Publisher != null && x.Publisher.Contains(search)) || (x.Version != null && x.Version.Contains(search)));
        }
        var total = await documents.LongCountAsync(ct);
        var ordered = (query.Sort, query.Descending) switch
        {
            ("publisher", false) => documents.OrderBy(x => x.Publisher).ThenBy(x => x.Title),
            ("publisher", true) => documents.OrderByDescending(x => x.Publisher).ThenBy(x => x.Title),
            ("version", false) => documents.OrderBy(x => x.Version).ThenBy(x => x.Title),
            ("version", true) => documents.OrderByDescending(x => x.Version).ThenBy(x => x.Title),
            ("usageState", false) => documents.OrderBy(x => x.UsageState).ThenBy(x => x.Title),
            ("usageState", true) => documents.OrderByDescending(x => x.UsageState).ThenBy(x => x.Title),
            ("title", true) => documents.OrderByDescending(x => x.Title),
            _ => documents.OrderBy(x => x.Title)
        };
        var items = await ordered.Skip(query.Skip).Take(query.Take).Select(x => new DocumentListItem(x.DocumentId, x.Title, x.Publisher, x.Version, x.UsageState, x.ConcurrencyVersion)).ToListAsync(ct);
        return Result<DocumentPage>.Success(new(items, total));
    }

    public async ValueTask<Result<DocumentDetails>> Handle(GetDocumentQuery query, CancellationToken ct)
    {
        var item = await db.Documents.AsNoTracking().Where(x => x.DocumentId == query.DocumentId).Select(x => new DocumentDetails(x.DocumentId, x.Title, x.Publisher, x.Version, x.PublicationDate, x.Source, x.UsageState, x.UsageStateReason, x.Notes, x.ConcurrencyVersion)).SingleOrDefaultAsync(ct);
        return item is null ? Result<DocumentDetails>.Failure(new AppError("DOCUMENT.NOT_FOUND", ErrorType.NotFound)) : Result<DocumentDetails>.Success(item);
    }

    public async ValueTask<Result<IReadOnlyList<CatalogVersionListItem>>> Handle(ListCatalogVersionsQuery query, CancellationToken ct)
    {
        if (!await db.Documents.AnyAsync(x => x.DocumentId == query.DocumentId, ct)) return Result<IReadOnlyList<CatalogVersionListItem>>.Failure(new AppError("DOCUMENT.NOT_FOUND", ErrorType.NotFound));
        var items = await db.CatalogVersions.AsNoTracking().Where(x => x.DocumentId == query.DocumentId).OrderByDescending(x => x.VersionNumber).Select(x => new CatalogVersionListItem(x.CatalogVersionId, x.VersionNumber, x.CatalogState, x.DraftRevision, x.CreatedAt, x.CreatedBy, x.SourceFileId != null, x.ConcurrencyVersion)).ToListAsync(ct);
        return Result<IReadOnlyList<CatalogVersionListItem>>.Success(items);
    }

    public async ValueTask<Result<CatalogVersionDetails>> Handle(GetCatalogVersionQuery query, CancellationToken ct)
    {
        var item = await db.CatalogVersions.AsNoTracking().Where(x => x.CatalogVersionId == query.CatalogVersionId).Select(x => new CatalogVersionDetails(x.CatalogVersionId, x.DocumentId, x.VersionNumber, x.CatalogState, x.DraftRevision, x.Notes, x.SourceFileId != null, x.ConcurrencyVersion)).SingleOrDefaultAsync(ct);
        return item is null ? Result<CatalogVersionDetails>.Failure(new AppError("CATALOG.NOT_FOUND", ErrorType.NotFound)) : Result<CatalogVersionDetails>.Success(item);
    }

    public async ValueTask<Result<CatalogEditorDetails>> Handle(GetCatalogEditorQuery query, CancellationToken ct)
    {
        var catalog = await Handle(new GetCatalogVersionQuery(query.CatalogVersionId), ct); if (!catalog.IsSuccess) return Result<CatalogEditorDetails>.Failure(catalog.Errors[0]);
        var elements = await db.DocumentElements.AsNoTracking().Where(x => x.CatalogVersionId == query.CatalogVersionId).OrderBy(x => x.ParentElementId).ThenBy(x => x.SortOrder).ToListAsync(ct);
        var elementIds = elements.Select(x => x.ElementId).ToArray();
        var questions = await db.Questions.AsNoTracking().Where(x => elementIds.Contains(x.ElementId)).OrderBy(x => x.SortOrder).ToListAsync(ct);
        var questionIds = questions.Select(x => x.QuestionId).ToArray();
        var scopes = await db.QuestionScopeTypes.AsNoTracking().Where(x => questionIds.Contains(x.QuestionId)).ToListAsync(ct);
        var weights = await db.DocumentElementWeights.AsNoTracking().Where(x => elementIds.Contains(x.ElementId)).ToDictionaryAsync(x => x.ElementId, x => x.Weight, ct);
        var editorElements = elements.Select(e => new DocumentElementEditorItem(e.ElementId, e.ParentElementId, e.SortOrder, e.Title, e.Text, e.Notes, weights.GetValueOrDefault(e.ElementId, 3), questions.Where(q => q.ElementId == e.ElementId).Select(q => new QuestionEditorItem(q.QuestionId, q.SortOrder, q.Text, q.VerificationHint, q.EvidenceHint, q.Notes, scopes.Where(s => s.QuestionId == q.QuestionId).Select(s => s.ScopeTypeId).ToArray())).ToArray())).ToArray();
        var scopeTypes = await db.ScopeTypes.AsNoTracking().OrderBy(x => x.Key).Select(x => new ScopeTypeItem(x.ScopeTypeId, x.Key, x.Name, x.Description)).ToListAsync(ct);
        return Result<CatalogEditorDetails>.Success(new(catalog.Value!, editorElements, scopeTypes));
    }
}
