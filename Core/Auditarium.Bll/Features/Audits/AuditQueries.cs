// SPDX-License-Identifier: MIT
using System.Text.Json;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Features.Catalog;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Audits;

public sealed record AuditUnitListItem(long AuditUnitId, long? ParentAuditUnitId, string? ParentName, long ScopeTypeId, string ScopeTypeKey, string ScopeTypeName, string Name, AuditUnitUsageState UsageState, long ConcurrencyVersion);
public sealed record AuditUnitDetails(long AuditUnitId, long? ParentAuditUnitId, long ScopeTypeId, string Name, string? Description, AuditUnitUsageState UsageState, string? UsageStateReason, string? Notes, long ConcurrencyVersion);
public sealed record AuditUnitPage(IReadOnlyList<AuditUnitListItem> Items, long TotalCount);
public sealed record AuditUnitHierarchyItem(long AuditUnitId, long? ParentAuditUnitId, int Depth, string Name, string ScopeTypeName, AuditUnitUsageState UsageState);
public sealed record AuditListItem(long AuditId, string Name, long AuditUnitId, string AuditUnitName, long CatalogVersionId, string DocumentTitle, int CatalogVersionNumber, AuditState AuditState, DateTimeOffset CreatedAt, long? AssignedAuditorUserId, string? AssignedAuditorDisplayName, long ConcurrencyVersion);
public sealed record AuditQuestionDetails(long AuditQuestionId, string Text, string? VerificationHint, string? EvidenceHint, AuditQuestionResult? Result, string? Comment, string? Evidence, DateTimeOffset? AnsweredAt, string? AnsweredByDisplayName, long ConcurrencyVersion);
public sealed record AuditorOption(long UserId, string DisplayName);
public sealed record AuditDetails(long AuditId, string Name, string? Description, long AuditUnitId, long CatalogVersionId, AuditState AuditState, AuditSettings Settings, string? Notes, string? AuditUnitContext, long? AssignedAuditorUserId, string? AssignedAuditorDisplayName, string? StateReason, IReadOnlyList<AuditQuestionDetails> Questions, long ConcurrencyVersion);
public sealed record AuditPage(IReadOnlyList<AuditListItem> Items, long TotalCount);
public sealed record AuditConfigurationOptions(IReadOnlyList<AuditUnitListItem> AuditUnits, IReadOnlyList<CatalogVersionOption> CatalogVersions, IReadOnlyList<ScopeTypeItem> ScopeTypes);
public sealed record CatalogVersionOption(long CatalogVersionId, string DocumentTitle, int VersionNumber, CatalogState CatalogState);
public sealed record AuditUnitFormOptions(IReadOnlyList<AuditUnitListItem> AuditUnits, IReadOnlyList<ScopeTypeItem> ScopeTypes);

[RequiresPermission("AuditUnits.Manage")] public sealed record ListAuditUnitsQuery(string? Search, int Skip, int Take, string Sort, bool Descending) : IRequest<Result<AuditUnitPage>>;
[RequiresPermission("AuditUnits.Manage")] public sealed record GetAuditUnitQuery(long AuditUnitId) : IRequest<Result<AuditUnitDetails>>;
[RequiresPermission("AuditUnits.Manage")] public sealed record GetAuditUnitHierarchyQuery() : IRequest<Result<IReadOnlyList<AuditUnitHierarchyItem>>>;
[RequiresPermission("AuditUnits.Manage")] public sealed record GetAuditUnitFormOptionsQuery() : IRequest<Result<AuditUnitFormOptions>>;
[RequiresPermission("Audits.Read")] public sealed record ListAuditsQuery(string? Search, AuditState? State, int Skip, int Take, string Sort, bool Descending) : IRequest<Result<AuditPage>>;
[RequiresPermission("Audits.Read")] public sealed record GetAuditQuery(long AuditId) : IRequest<Result<AuditDetails>>;
[RequiresPermission("Audits.Assign")] public sealed record GetAuditorOptionsQuery() : IRequest<Result<IReadOnlyList<AuditorOption>>>;
[RequiresPermission("Audits.Create")] public sealed record GetAuditConfigurationOptionsQuery() : IRequest<Result<AuditConfigurationOptions>>;

public sealed class AuditQueryHandler(IAuditariumDbContext db) :
    IRequestHandler<ListAuditUnitsQuery, Result<AuditUnitPage>>, IRequestHandler<GetAuditUnitQuery, Result<AuditUnitDetails>>, IRequestHandler<GetAuditUnitHierarchyQuery, Result<IReadOnlyList<AuditUnitHierarchyItem>>>, IRequestHandler<GetAuditUnitFormOptionsQuery, Result<AuditUnitFormOptions>>,
    IRequestHandler<ListAuditsQuery, Result<AuditPage>>, IRequestHandler<GetAuditQuery, Result<AuditDetails>>, IRequestHandler<GetAuditorOptionsQuery, Result<IReadOnlyList<AuditorOption>>>, IRequestHandler<GetAuditConfigurationOptionsQuery, Result<AuditConfigurationOptions>>
{
    public async ValueTask<Result<AuditUnitPage>> Handle(ListAuditUnitsQuery query, CancellationToken ct)
    {
        if (query.Skip < 0 || query.Take is < 1 or > 200 || query.Sort is not ("name" or "scopeType" or "usageState")) return Fail<AuditUnitPage>("AUDIT_UNIT.LIST_ARGUMENT_INVALID");
        var units = db.AuditUnits.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            units = units.Where(x => x.Name.Contains(search) || (x.Description != null && x.Description.Contains(search)));
        }
        var total = await units.LongCountAsync(ct);
        var joined = from unit in units
                     join scope in db.ScopeTypes.AsNoTracking() on unit.ScopeTypeId equals scope.ScopeTypeId
                     join parent in db.AuditUnits.AsNoTracking() on unit.ParentAuditUnitId equals parent.AuditUnitId into parents
                     from parent in parents.DefaultIfEmpty()
                     select new AuditUnitListItem(unit.AuditUnitId, unit.ParentAuditUnitId, parent == null ? null : parent.Name, unit.ScopeTypeId, scope.Key, scope.Name, unit.Name, unit.UsageState, unit.ConcurrencyVersion);
        var ordered = (query.Sort, query.Descending) switch
        {
            ("scopeType", false) => joined.OrderBy(x => x.ScopeTypeName).ThenBy(x => x.Name),
            ("scopeType", true) => joined.OrderByDescending(x => x.ScopeTypeName).ThenBy(x => x.Name),
            ("usageState", false) => joined.OrderBy(x => x.UsageState).ThenBy(x => x.Name),
            ("usageState", true) => joined.OrderByDescending(x => x.UsageState).ThenBy(x => x.Name),
            ("name", true) => joined.OrderByDescending(x => x.Name),
            _ => joined.OrderBy(x => x.Name)
        };
        return Result<AuditUnitPage>.Success(new(await ordered.Skip(query.Skip).Take(query.Take).ToListAsync(ct), total));
    }

    public async ValueTask<Result<AuditUnitDetails>> Handle(GetAuditUnitQuery query, CancellationToken ct)
    {
        var unit = await db.AuditUnits.AsNoTracking().Where(x => x.AuditUnitId == query.AuditUnitId)
            .Select(x => new AuditUnitDetails(x.AuditUnitId, x.ParentAuditUnitId, x.ScopeTypeId, x.Name, x.Description, x.UsageState, x.UsageStateReason, x.Notes, x.ConcurrencyVersion)).SingleOrDefaultAsync(ct);
        return unit is null ? Fail<AuditUnitDetails>("AUDIT_UNIT.NOT_FOUND", ErrorType.NotFound) : Result<AuditUnitDetails>.Success(unit);
    }

    public async ValueTask<Result<IReadOnlyList<AuditUnitHierarchyItem>>> Handle(GetAuditUnitHierarchyQuery query, CancellationToken ct)
    {
        var units = await (from unit in db.AuditUnits.AsNoTracking()
                           join scope in db.ScopeTypes.AsNoTracking() on unit.ScopeTypeId equals scope.ScopeTypeId
                           select new AuditUnitHierarchyItem(unit.AuditUnitId, unit.ParentAuditUnitId, 0, unit.Name, scope.Name, unit.UsageState)).OrderBy(x => x.Name).ToListAsync(ct);
        var byParent = units.ToLookup(x => x.ParentAuditUnitId);
        var result = new List<AuditUnitHierarchyItem>();
        void AddChildren(long? parent, int depth)
        {
            foreach (var unit in byParent[parent]) { result.Add(unit with { Depth = depth }); AddChildren(unit.AuditUnitId, depth + 1); }
        }
        AddChildren(null, 0);
        return Result<IReadOnlyList<AuditUnitHierarchyItem>>.Success(result);
    }

    public async ValueTask<Result<AuditUnitFormOptions>> Handle(GetAuditUnitFormOptionsQuery query, CancellationToken ct)
    {
        var units = (await Handle(new ListAuditUnitsQuery(null, 0, 200, "name", false), ct)).Value?.Items ?? [];
        var scopeTypes = await db.ScopeTypes.AsNoTracking().OrderBy(x => x.Key).Select(x => new ScopeTypeItem(x.ScopeTypeId, x.Key, x.Name, x.Description)).ToListAsync(ct);
        return Result<AuditUnitFormOptions>.Success(new(units, scopeTypes));
    }

    public async ValueTask<Result<AuditPage>> Handle(ListAuditsQuery query, CancellationToken ct)
    {
        if (query.Skip < 0 || query.Take is < 1 or > 200 || query.Sort is not ("name" or "createdAt" or "state")) return Fail<AuditPage>("AUDIT.LIST_ARGUMENT_INVALID");
        var audits = db.Audits.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search)) { var search = query.Search.Trim(); audits = audits.Where(x => x.Name.Contains(search) || (x.Description != null && x.Description.Contains(search))); }
        if (query.State is { } state) audits = audits.Where(x => x.AuditState == state);
        var total = await audits.LongCountAsync(ct);
        var joined = from audit in audits
                     join unit in db.AuditUnits.AsNoTracking() on audit.AuditUnitId equals unit.AuditUnitId
                     join catalog in db.CatalogVersions.AsNoTracking() on audit.CatalogVersionId equals catalog.CatalogVersionId
                     join document in db.Documents.AsNoTracking() on catalog.DocumentId equals document.DocumentId
                     join auditor in db.Users.AsNoTracking() on audit.AssignedAuditorUserId equals auditor.UserId into auditors
                     from auditor in auditors.DefaultIfEmpty()
                     select new AuditListItem(audit.AuditId, audit.Name, audit.AuditUnitId, unit.Name, audit.CatalogVersionId, document.Title, catalog.VersionNumber, audit.AuditState, audit.CreatedAt, audit.AssignedAuditorUserId, auditor == null ? null : auditor.DisplayName, audit.ConcurrencyVersion);
        var ordered = (query.Sort, query.Descending) switch
        {
            ("createdAt", false) => joined.OrderBy(x => x.CreatedAt),
            ("createdAt", true) => joined.OrderByDescending(x => x.CreatedAt),
            ("state", false) => joined.OrderBy(x => x.AuditState).ThenBy(x => x.Name),
            ("state", true) => joined.OrderByDescending(x => x.AuditState).ThenBy(x => x.Name),
            ("name", true) => joined.OrderByDescending(x => x.Name),
            _ => joined.OrderBy(x => x.Name)
        };
        return Result<AuditPage>.Success(new(await ordered.Skip(query.Skip).Take(query.Take).ToListAsync(ct), total));
    }

    public async ValueTask<Result<AuditDetails>> Handle(GetAuditQuery query, CancellationToken ct)
    {
        var audit = await (from item in db.Audits.AsNoTracking().Where(x => x.AuditId == query.AuditId)
                           join auditor in db.Users.AsNoTracking() on item.AssignedAuditorUserId equals auditor.UserId into auditors
                           from auditor in auditors.DefaultIfEmpty()
                           select new { item, AssignedAuditorDisplayName = auditor == null ? null : auditor.DisplayName }).SingleOrDefaultAsync(ct);
        if (audit is null) return Fail<AuditDetails>("AUDIT.NOT_FOUND", ErrorType.NotFound);
        var questions = await (from question in db.AuditQuestions.AsNoTracking()
                               join element in db.AuditDocumentElements.AsNoTracking() on question.AuditDocumentElementId equals element.AuditDocumentElementId
                               join definition in db.Questions.AsNoTracking() on question.QuestionId equals definition.QuestionId
                               join answeredBy in db.Users.AsNoTracking() on question.AnsweredBy equals answeredBy.UserId into answerers
                               from answeredBy in answerers.DefaultIfEmpty()
                               where element.AuditId == audit.item.AuditId
                               orderby element.AuditDocumentElementId, definition.SortOrder
                               select new AuditQuestionDetails(question.AuditQuestionId, definition.Text, definition.VerificationHint, definition.EvidenceHint, question.Result, question.Comment, question.Evidence, question.AnsweredAt, answeredBy == null ? null : answeredBy.DisplayName, question.ConcurrencyVersion)).ToListAsync(ct);
        return Result<AuditDetails>.Success(new(audit.item.AuditId, audit.item.Name, audit.item.Description, audit.item.AuditUnitId, audit.item.CatalogVersionId, audit.item.AuditState, Deserialize(audit.item.AuditSettings), audit.item.Notes, audit.item.AuditUnitContext, audit.item.AssignedAuditorUserId, audit.AssignedAuditorDisplayName, audit.item.StateReason, questions, audit.item.ConcurrencyVersion));
    }

    public async ValueTask<Result<IReadOnlyList<AuditorOption>>> Handle(GetAuditorOptionsQuery query, CancellationToken ct)
    {
        var auditors = await (from user in db.Users.AsNoTracking()
                              join userRole in db.UserRoles.AsNoTracking() on user.UserId equals userRole.UserId
                              join rolePermission in db.RolePermissions.AsNoTracking() on userRole.RoleId equals rolePermission.RoleId
                              where user.IsActive && rolePermission.PermissionKey == "Audits.Answer"
                              orderby user.DisplayName
                              select new AuditorOption(user.UserId, user.DisplayName)).Distinct().ToListAsync(ct);
        return Result<IReadOnlyList<AuditorOption>>.Success(auditors);
    }

    public async ValueTask<Result<AuditConfigurationOptions>> Handle(GetAuditConfigurationOptionsQuery query, CancellationToken ct)
    {
        var units = (await Handle(new ListAuditUnitsQuery(null, 0, 200, "name", false), ct)).Value?.Items ?? [];
        var catalogs = await (from catalog in db.CatalogVersions.AsNoTracking()
                              join document in db.Documents.AsNoTracking() on catalog.DocumentId equals document.DocumentId
                              orderby document.Title, catalog.VersionNumber descending
                              select new CatalogVersionOption(catalog.CatalogVersionId, document.Title, catalog.VersionNumber, catalog.CatalogState)).ToListAsync(ct);
        var scopeTypes = await db.ScopeTypes.AsNoTracking().OrderBy(x => x.Key).Select(x => new ScopeTypeItem(x.ScopeTypeId, x.Key, x.Name, x.Description)).ToListAsync(ct);
        return Result<AuditConfigurationOptions>.Success(new(units, catalogs, scopeTypes));
    }

    private static AuditSettings Deserialize(string value) => JsonSerializer.Deserialize<AuditSettings>(value) ?? new(new Dictionary<AuditQuestionResult, ResponseRule>());
    private static Result<T> Fail<T>(string code, ErrorType type = ErrorType.Validation) => Result<T>.Failure(new AppError(code, type));
}
