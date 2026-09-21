// SPDX-License-Identifier: MIT
using System.Text.Json;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Audits;

public sealed record AuditUnitInput(long? ParentAuditUnitId, long ScopeTypeId, string Name, string? Description, AuditUnitUsageState UsageState, string? UsageStateReason, string? Notes);
public sealed record ResponseRule(bool CommentRequired, bool EvidenceRequired);
public sealed record AuditSettings(IReadOnlyDictionary<AuditQuestionResult, ResponseRule> ResponsePolicy);
public sealed record AuditInput(string Name, string? Description, long AuditUnitId, long CatalogVersionId, AuditSettings? Settings, string? Notes);
public sealed record AuditPreview(int DocumentElementCount, int QuestionCount);

[RequiresPermission("AuditUnits.Manage")] public sealed record CreateAuditUnitCommand(AuditUnitInput Input) : IRequest<Result<long>>;
[RequiresPermission("AuditUnits.Manage")] public sealed record UpdateAuditUnitCommand(long AuditUnitId, AuditUnitInput Input, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Audits.Create")] public sealed record CreateAuditCommand(AuditInput Input) : IRequest<Result<long>>;
[RequiresPermission("Audits.UpdateDraft")] public sealed record UpdateAuditDraftCommand(long AuditId, AuditInput Input, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Audits.Read")] public sealed record GetAuditPreviewQuery(long AuditId) : IRequest<Result<AuditPreview>>;
[RequiresPermission("Audits.Publish")] public sealed record PublishAuditCommand(long AuditId, long? ConcurrencyVersion = null) : IRequest<Result>;
[RequiresPermission("Audits.Claim")] public sealed record ClaimAuditCommand(long AuditId, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Audits.ReleaseOwn")] public sealed record ReleaseOwnAuditCommand(long AuditId, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Audits.Assign")] public sealed record AssignAuditorCommand(long AuditId, long? UserId, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Audits.Answer")] public sealed record AnswerAuditQuestionCommand(long AuditQuestionId, AuditQuestionResult Result, string? Comment, string? Evidence, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Audits.Answer")] public sealed record ResetAuditQuestionCommand(long AuditQuestionId, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Audits.Finalize")] public sealed record FinalizeAuditCommand(long AuditId, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Audits.Cancel")] public sealed record CancelAuditCommand(long AuditId, string Reason, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Audits.Reopen")] public sealed record ReopenAuditCommand(long AuditId, string Reason, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Audits.Create")] public sealed record CreateAuditRepeatCommand(long SourceAuditId, AuditInput Input) : IRequest<Result<long>>;

public sealed class AuditCommandHandler(IAuditariumDbContext db, ICurrentActor actor) :
    IRequestHandler<CreateAuditUnitCommand, Result<long>>, IRequestHandler<UpdateAuditUnitCommand, Result>, IRequestHandler<CreateAuditCommand, Result<long>>, IRequestHandler<UpdateAuditDraftCommand, Result>, IRequestHandler<GetAuditPreviewQuery, Result<AuditPreview>>, IRequestHandler<PublishAuditCommand, Result>, IRequestHandler<ClaimAuditCommand, Result>, IRequestHandler<ReleaseOwnAuditCommand, Result>, IRequestHandler<AssignAuditorCommand, Result>, IRequestHandler<AnswerAuditQuestionCommand, Result>, IRequestHandler<ResetAuditQuestionCommand, Result>, IRequestHandler<FinalizeAuditCommand, Result>, IRequestHandler<CancelAuditCommand, Result>, IRequestHandler<ReopenAuditCommand, Result>, IRequestHandler<CreateAuditRepeatCommand, Result<long>>
{
    private static readonly AuditSettings DefaultSettings = new(new Dictionary<AuditQuestionResult, ResponseRule> { [AuditQuestionResult.Yes] = new(false, false), [AuditQuestionResult.No] = new(false, false), [AuditQuestionResult.NotApplicable] = new(true, false), [AuditQuestionResult.NotDeterminable] = new(true, false) });

    public async ValueTask<Result<long>> Handle(CreateAuditUnitCommand m, CancellationToken ct)
    {
        var error = await ValidateUnitAsync(m.Input, null, ct); if (error is not null) return Fail<long>(error);
        var unit = new AuditUnit { ParentAuditUnitId = m.Input.ParentAuditUnitId, ScopeTypeId = m.Input.ScopeTypeId, Name = m.Input.Name.Trim(), Description = Null(m.Input.Description), UsageState = m.Input.UsageState, UsageStateReason = Null(m.Input.UsageStateReason), Notes = Null(m.Input.Notes) };
        db.AuditUnits.Add(unit); await db.SaveChangesAsync(ct); return Result<long>.Success(unit.AuditUnitId);
    }

    public async ValueTask<Result> Handle(UpdateAuditUnitCommand m, CancellationToken ct)
    {
        var unit = await db.AuditUnits.SingleOrDefaultAsync(x => x.AuditUnitId == m.AuditUnitId, ct); if (unit is null) return NotFound("AUDIT_UNIT.NOT_FOUND"); if (unit.ConcurrencyVersion != m.ConcurrencyVersion) return Conflict("AUDIT_UNIT.CONCURRENCY_CONFLICT");
        var error = await ValidateUnitAsync(m.Input, unit.AuditUnitId, ct); if (error is not null) return Fail(error);
        unit.ParentAuditUnitId = m.Input.ParentAuditUnitId; unit.ScopeTypeId = m.Input.ScopeTypeId; unit.Name = m.Input.Name.Trim(); unit.Description = Null(m.Input.Description); unit.UsageState = m.Input.UsageState; unit.UsageStateReason = Null(m.Input.UsageStateReason); unit.Notes = Null(m.Input.Notes);
        await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result<long>> Handle(CreateAuditCommand m, CancellationToken ct) => await CreateDraftAsync(m.Input, null, ct);

    public async ValueTask<Result<long>> Handle(CreateAuditRepeatCommand m, CancellationToken ct)
    {
        var source = await db.Audits.AsNoTracking().SingleOrDefaultAsync(x => x.AuditId == m.SourceAuditId, ct); if (source is null) return Fail<long>("AUDIT.NOT_FOUND", ErrorType.NotFound);
        return await CreateDraftAsync(m.Input, source.OriginAuditId ?? source.AuditId, ct);
    }

    public async ValueTask<Result> Handle(UpdateAuditDraftCommand m, CancellationToken ct)
    {
        var audit = await db.Audits.SingleOrDefaultAsync(x => x.AuditId == m.AuditId, ct); if (audit is null) return NotFound("AUDIT.NOT_FOUND"); if (audit.AuditState != AuditState.Draft) return Conflict("AUDIT.NOT_DRAFT"); if (audit.ConcurrencyVersion != m.ConcurrencyVersion) return Conflict("AUDIT.CONCURRENCY_CONFLICT");
        var error = await ValidateDraftInputAsync(m.Input, ct); if (error is not null) return Fail(error);
        audit.Name = m.Input.Name.Trim(); audit.Description = Null(m.Input.Description); audit.AuditUnitId = m.Input.AuditUnitId; audit.CatalogVersionId = m.Input.CatalogVersionId; audit.AuditSettings = Serialize(m.Input.Settings ?? DefaultSettings); audit.Notes = Null(m.Input.Notes); await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result<AuditPreview>> Handle(GetAuditPreviewQuery m, CancellationToken ct)
    {
        var audit = await db.Audits.AsNoTracking().SingleOrDefaultAsync(x => x.AuditId == m.AuditId, ct); if (audit is null) return Fail<AuditPreview>("AUDIT.NOT_FOUND", ErrorType.NotFound);
        var matches = await MatchingQuestionsAsync(audit.AuditUnitId, audit.CatalogVersionId, ct); return Result<AuditPreview>.Success(new(matches.Select(x => x.ElementId).Distinct().Count(), matches.Count));
    }

    public async ValueTask<Result> Handle(PublishAuditCommand m, CancellationToken ct)
    {
        var audit = await db.Audits.SingleOrDefaultAsync(x => x.AuditId == m.AuditId, ct); if (audit is null) return NotFound("AUDIT.NOT_FOUND"); if (audit.AuditState != AuditState.Draft) return Conflict("AUDIT.NOT_DRAFT"); if (m.ConcurrencyVersion is { } version && audit.ConcurrencyVersion != version) return Conflict("AUDIT.CONCURRENCY_CONFLICT");
        var input = new AuditInput(audit.Name, audit.Description, audit.AuditUnitId, audit.CatalogVersionId, Deserialize(audit.AuditSettings), audit.Notes); var error = await ValidateDraftInputAsync(input, ct); if (error is not null) return Fail(error);
        var unit = await db.AuditUnits.SingleAsync(x => x.AuditUnitId == audit.AuditUnitId, ct); if (unit.UsageState != AuditUnitUsageState.Active) return Fail("AUDIT.AUDIT_UNIT_INACTIVE");
        var catalog = await db.CatalogVersions.SingleAsync(x => x.CatalogVersionId == audit.CatalogVersionId, ct); var document = await db.Documents.SingleAsync(x => x.DocumentId == catalog.DocumentId, ct);
        if (catalog.CatalogState != CatalogState.Ready || document.UsageState != DocumentUsageState.Active) return Fail("AUDIT.CATALOG_NOT_USABLE");
        var matches = await MatchingQuestionsAsync(unit.AuditUnitId, catalog.CatalogVersionId, ct); if (matches.Count == 0) return Fail("AUDIT.QUESTIONS_REQUIRED");
        var weights = await db.DocumentElementWeights.Where(x => matches.Select(q => q.ElementId).Contains(x.ElementId)).ToDictionaryAsync(x => x.ElementId, x => x.Weight, ct);
        await using var transaction = await db.BeginTransactionAsync(ct);
        try
        {
            foreach (var group in matches.GroupBy(x => x.ElementId))
            {
                var element = new AuditDocumentElement { AuditId = audit.AuditId, ElementId = group.Key, WeightSnapshot = weights.GetValueOrDefault(group.Key, 3) };
                db.AuditDocumentElements.Add(element);
                await db.SaveChangesAsync(ct);
                foreach (var question in group) db.AuditQuestions.Add(new AuditQuestion { AuditDocumentElementId = element.AuditDocumentElementId, QuestionId = question.QuestionId });
            }
            audit.AuditUnitContext = await ContextAsync(unit, ct); audit.AuditState = AuditState.Ready;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Result.Success();
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async ValueTask<Result> Handle(ClaimAuditCommand m, CancellationToken ct)
    {
        if (actor.UserId is not { } userId) return Fail("AUDIT.ACTOR_REQUIRED", ErrorType.Forbidden); var audit = await db.Audits.SingleOrDefaultAsync(x => x.AuditId == m.AuditId, ct); if (audit is null) return NotFound("AUDIT.NOT_FOUND"); if (audit.ConcurrencyVersion != m.ConcurrencyVersion) return Conflict("AUDIT.ALREADY_ASSIGNED"); if (audit.AuditState is not (AuditState.Ready or AuditState.InProgress) || audit.AssignedAuditorUserId is not null) return Conflict("AUDIT.ALREADY_ASSIGNED"); audit.AssignedAuditorUserId = userId; return await SaveConcurrencyAsync("AUDIT.ALREADY_ASSIGNED", ct);
    }

    public async ValueTask<Result> Handle(ReleaseOwnAuditCommand m, CancellationToken ct)
    {
        var audit = await db.Audits.SingleOrDefaultAsync(x => x.AuditId == m.AuditId, ct); if (audit is null) return NotFound("AUDIT.NOT_FOUND"); if (audit.ConcurrencyVersion != m.ConcurrencyVersion) return Conflict("AUDIT.CONCURRENCY_CONFLICT"); if (actor.UserId is null || audit.AssignedAuditorUserId != actor.UserId) return Fail("AUDIT.NOT_ASSIGNED_TO_ACTOR", ErrorType.Forbidden); audit.AssignedAuditorUserId = null; await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result> Handle(AssignAuditorCommand m, CancellationToken ct)
    {
        var audit = await db.Audits.SingleOrDefaultAsync(x => x.AuditId == m.AuditId, ct); if (audit is null) return NotFound("AUDIT.NOT_FOUND"); if (audit.ConcurrencyVersion != m.ConcurrencyVersion) return Conflict("AUDIT.CONCURRENCY_CONFLICT"); if (audit.AuditState is not (AuditState.Ready or AuditState.InProgress)) return Conflict("AUDIT.NOT_ASSIGNABLE");
        if (m.UserId is { } user && !await IsAuditorAsync(user, ct)) return Fail("AUDIT.ASSIGNEE_INVALID", ErrorType.Validation); audit.AssignedAuditorUserId = m.UserId; await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result> Handle(AnswerAuditQuestionCommand m, CancellationToken ct)
    {
        var q = await db.AuditQuestions.SingleOrDefaultAsync(x => x.AuditQuestionId == m.AuditQuestionId, ct); if (q is null) return NotFound("AUDIT_QUESTION.NOT_FOUND"); if (q.ConcurrencyVersion != m.ConcurrencyVersion) return Conflict("AUDIT_QUESTION.CONCURRENCY_CONFLICT"); var audit = await AuditForQuestionAsync(q.AuditDocumentElementId, ct); var access = await CanEditAsync(audit, ct); if (access is not null) return Fail(access, ErrorType.Forbidden);
        var settings = Deserialize(audit.AuditSettings); var rule = settings.ResponsePolicy[m.Result]; if ((rule.CommentRequired && Empty(m.Comment)) || (rule.EvidenceRequired && Empty(m.Evidence))) return Fail("AUDIT_QUESTION.RESPONSE_POLICY_VIOLATION"); q.Result = m.Result; q.Comment = Null(m.Comment); q.Evidence = Null(m.Evidence); q.AnsweredAt = DateTimeOffset.UtcNow; q.AnsweredBy = actor.UserId; if (audit.AuditState == AuditState.Ready) audit.AuditState = AuditState.InProgress; await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result> Handle(ResetAuditQuestionCommand m, CancellationToken ct)
    {
        var q = await db.AuditQuestions.SingleOrDefaultAsync(x => x.AuditQuestionId == m.AuditQuestionId, ct); if (q is null) return NotFound("AUDIT_QUESTION.NOT_FOUND"); if (q.ConcurrencyVersion != m.ConcurrencyVersion) return Conflict("AUDIT_QUESTION.CONCURRENCY_CONFLICT"); var audit = await AuditForQuestionAsync(q.AuditDocumentElementId, ct); var access = await CanEditAsync(audit, ct); if (access is not null) return Fail(access, ErrorType.Forbidden); q.Result = null; q.Comment = null; q.Evidence = null; q.AnsweredAt = null; q.AnsweredBy = null; if (audit.AuditState == AuditState.InProgress && !await db.AuditQuestions.Where(x => db.AuditDocumentElements.Where(e => e.AuditId == audit.AuditId).Select(e => e.AuditDocumentElementId).Contains(x.AuditDocumentElementId) && x.AuditQuestionId != q.AuditQuestionId).AnyAsync(x => x.Result != null, ct)) audit.AuditState = AuditState.Ready; await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result> Handle(FinalizeAuditCommand m, CancellationToken ct)
    {
        var audit = await db.Audits.SingleOrDefaultAsync(x => x.AuditId == m.AuditId, ct); if (audit is null) return NotFound("AUDIT.NOT_FOUND"); if (audit.ConcurrencyVersion != m.ConcurrencyVersion) return Conflict("AUDIT.CONCURRENCY_CONFLICT"); var access = await CanEditAsync(audit, ct); if (access is not null) return Fail(access, ErrorType.Forbidden); if (audit.AuditState != AuditState.InProgress) return Conflict("AUDIT.NOT_IN_PROGRESS"); var questions = await QuestionsForAuditAsync(audit.AuditId, ct); if (questions.Any(x => x.Result is null)) return Fail("AUDIT.FINALIZATION_INCOMPLETE"); audit.AuditState = AuditState.Finalized; audit.AssignedAuditorUserId = null; await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result> Handle(CancelAuditCommand m, CancellationToken ct) => await ChangeCanceledAsync(m.AuditId, m.Reason, m.ConcurrencyVersion, ct);
    public async ValueTask<Result> Handle(ReopenAuditCommand m, CancellationToken ct)
    {
        if (Empty(m.Reason)) return Fail("AUDIT.REOPEN_REASON_REQUIRED"); var audit = await db.Audits.SingleOrDefaultAsync(x => x.AuditId == m.AuditId, ct); if (audit is null) return NotFound("AUDIT.NOT_FOUND"); if (audit.ConcurrencyVersion != m.ConcurrencyVersion) return Conflict("AUDIT.CONCURRENCY_CONFLICT"); if (audit.AuditState != AuditState.Canceled) return Conflict("AUDIT.NOT_CANCELED"); audit.AuditState = (await QuestionsForAuditAsync(audit.AuditId, ct)).Any(x => x.Result != null) ? AuditState.InProgress : AuditState.Ready; audit.StateReason = m.Reason.Trim(); await db.SaveChangesAsync(ct); return Result.Success();
    }

    private async Task<Result<long>> CreateDraftAsync(AuditInput input, long? origin, CancellationToken ct) { var error = await ValidateDraftInputAsync(input, ct); if (error is not null) return Fail<long>(error); var audit = new Audit { OriginAuditId = origin, Name = input.Name.Trim(), Description = Null(input.Description), AuditUnitId = input.AuditUnitId, CatalogVersionId = input.CatalogVersionId, AuditSettings = Serialize(input.Settings ?? DefaultSettings), CreatedAt = DateTimeOffset.UtcNow, Notes = Null(input.Notes) }; db.Audits.Add(audit); await db.SaveChangesAsync(ct); return Result<long>.Success(audit.AuditId); }
    private async Task<string?> ValidateDraftInputAsync(AuditInput input, CancellationToken ct) { if (Empty(input.Name)) return "AUDIT.NAME_REQUIRED"; if (!await db.AuditUnits.AnyAsync(x => x.AuditUnitId == input.AuditUnitId, ct) || !await db.CatalogVersions.AnyAsync(x => x.CatalogVersionId == input.CatalogVersionId, ct)) return "AUDIT.CONFIGURATION_REFERENCE_INVALID"; return ValidSettings(input.Settings ?? DefaultSettings) ? null : "AUDIT.SETTINGS_INVALID"; }
    private async Task<string?> ValidateUnitAsync(AuditUnitInput input, long? id, CancellationToken ct) { if (Empty(input.Name) || input.UsageState == AuditUnitUsageState.Inactive && Empty(input.UsageStateReason)) return "AUDIT_UNIT.INVALID"; if (input.ScopeTypeId == 13 && Empty(input.Description)) return "AUDIT_UNIT.OTHER_DESCRIPTION_REQUIRED"; if (!await db.ScopeTypes.AnyAsync(x => x.ScopeTypeId == input.ScopeTypeId, ct)) return "AUDIT_UNIT.SCOPE_TYPE_INVALID"; if (input.ParentAuditUnitId is not { } parent) return null; if (parent == id || !await db.AuditUnits.AnyAsync(x => x.AuditUnitId == parent, ct)) return "AUDIT_UNIT.PARENT_INVALID"; var parentUnit = await db.AuditUnits.SingleAsync(x => x.AuditUnitId == parent, ct); if (!ParentAllowed(input.ScopeTypeId, parentUnit.ScopeTypeId)) return "AUDIT_UNIT.PARENT_SCOPE_INVALID"; for (var current = parentUnit; current.ParentAuditUnitId is { } next;) { if (next == id) return "AUDIT_UNIT.PARENT_CYCLE"; current = await db.AuditUnits.SingleAsync(x => x.AuditUnitId == next, ct); } return null; }
    private static bool ParentAllowed(long child, long parent) => child switch { 1 => parent == 1, 2 => parent == 1, 3 => parent is 1 or 2, 4 => parent is 1 or 2 or 3 or 4, 5 => parent is 2 or 3 or 4, 6 => parent is 2 or 3 or 4, 7 => parent is 1 or 2 or 4 or 6, 8 => parent is 1 or 2 or 4 or 6 or 7, 9 => parent is 1 or 4 or 8 or 11, 10 or 11 => parent is 1 or 4, 12 => parent == 1, _ => false };
    private async Task<List<Question>> MatchingQuestionsAsync(long unitId, long catalogId, CancellationToken ct) { var unit = await db.AuditUnits.SingleOrDefaultAsync(x => x.AuditUnitId == unitId, ct); if (unit is null) return []; return await (from q in db.Questions join e in db.DocumentElements on q.ElementId equals e.ElementId join s in db.QuestionScopeTypes on q.QuestionId equals s.QuestionId where e.CatalogVersionId == catalogId && s.ScopeTypeId == unit.ScopeTypeId select q).Distinct().ToListAsync(ct); }
    private async Task<string> ContextAsync(AuditUnit unit, CancellationToken ct) { var scopeTypeId = unit.ScopeTypeId; var names = new List<string> { unit.Name }; while (unit.ParentAuditUnitId is { } parent) { unit = await db.AuditUnits.SingleAsync(x => x.AuditUnitId == parent, ct); names.Add(unit.Name); } return JsonSerializer.Serialize(new { name = names[0], scope_type_id = scopeTypeId, hierarchy_path = string.Join(" / ", names.AsEnumerable().Reverse()) }); }
    private async Task<bool> IsAuditorAsync(long userId, CancellationToken ct) => await (from u in db.Users join ur in db.UserRoles on u.UserId equals ur.UserId join rp in db.RolePermissions on ur.RoleId equals rp.RoleId where u.UserId == userId && u.IsActive && rp.PermissionKey == "Audits.Answer" select u.UserId).AnyAsync(ct);
    private async Task<Audit> AuditForQuestionAsync(long auditElementId, CancellationToken ct) { var auditId = await db.AuditDocumentElements.Where(x => x.AuditDocumentElementId == auditElementId).Select(x => x.AuditId).SingleAsync(ct); return await db.Audits.SingleAsync(x => x.AuditId == auditId, ct); }
    private async Task<string?> CanEditAsync(Audit audit, CancellationToken ct) => actor.UserId is null || audit.AssignedAuditorUserId != actor.UserId || audit.AuditState is not (AuditState.Ready or AuditState.InProgress) ? "AUDIT.NOT_ASSIGNED_TO_ACTOR" : null;
    private async Task<List<AuditQuestion>> QuestionsForAuditAsync(long auditId, CancellationToken ct) => await db.AuditQuestions.Where(x => db.AuditDocumentElements.Where(e => e.AuditId == auditId).Select(e => e.AuditDocumentElementId).Contains(x.AuditDocumentElementId)).ToListAsync(ct);
    private async Task<Result> ChangeCanceledAsync(long id, string reason, long version, CancellationToken ct) { if (Empty(reason)) return Fail("AUDIT.CANCEL_REASON_REQUIRED"); var audit = await db.Audits.SingleOrDefaultAsync(x => x.AuditId == id, ct); if (audit is null) return NotFound("AUDIT.NOT_FOUND"); if (audit.ConcurrencyVersion != version) return Conflict("AUDIT.CONCURRENCY_CONFLICT"); if (audit.AuditState is not (AuditState.Ready or AuditState.InProgress)) return Conflict("AUDIT.NOT_CANCELABLE"); audit.AuditState = AuditState.Canceled; audit.StateReason = reason.Trim(); audit.AssignedAuditorUserId = null; await db.SaveChangesAsync(ct); return Result.Success(); }
    private async Task<Result> SaveConcurrencyAsync(string code, CancellationToken ct) { try { await db.SaveChangesAsync(ct); return Result.Success(); } catch (DbUpdateConcurrencyException) { return Conflict(code); } }
    private static bool ValidSettings(AuditSettings s) => Enum.GetValues<AuditQuestionResult>().All(x => s.ResponsePolicy.ContainsKey(x));
    private static AuditSettings Deserialize(string value) => JsonSerializer.Deserialize<AuditSettings>(value) ?? DefaultSettings;
    private static string Serialize(AuditSettings value) => JsonSerializer.Serialize(value);
    private static bool Empty(string? value) => string.IsNullOrWhiteSpace(value); private static string? Null(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim(); private static Result Fail(string code, ErrorType type = ErrorType.Validation) => Result.Failure(new AppError(code, type)); private static Result<T> Fail<T>(string code, ErrorType type = ErrorType.Validation) => Result<T>.Failure(new AppError(code, type)); private static Result NotFound(string code) => Fail(code, ErrorType.NotFound); private static Result Conflict(string code) => Fail(code, ErrorType.Conflict);
}
