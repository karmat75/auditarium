// SPDX-License-Identifier: MIT
using System.Text.Json;
using System.Text.Json.Serialization;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Audits;

/// <summary>Explicit, shared selection criteria for audit data consumers.</summary>
public sealed record AuditDataFilter(
    DateTimeOffset? CreatedFrom = null,
    DateTimeOffset? CreatedTo = null,
    long? ScopeTypeId = null,
    long? AuditUnitId = null,
    long? DocumentId = null,
    long? CatalogVersionId = null,
    AuditState? AuditState = null);

public enum AuditDataAuditSort { CreatedAt, Name, State }
public enum AuditDataElementSort { CreatedAt, ElementTitle, Weight }
public enum AuditDataQuestionSort { CreatedAt, QuestionText, AnsweredAt }
public enum AuditDocumentElementResult { Fulfilled, NotFulfilled, NotDeterminable, NotApplicable }

public sealed record AuditDataContext(
    long AuditId,
    string AuditName,
    string? AuditDescription,
    DateTimeOffset CreatedAt,
    AuditState AuditState,
    long AuditUnitId,
    string AuditUnitName,
    string? AuditUnitHierarchyPath,
    long ScopeTypeId,
    string ScopeTypeKey,
    string ScopeTypeName,
    long DocumentId,
    string DocumentTitle,
    string? DocumentVersion,
    long CatalogVersionId,
    int CatalogVersionNumber);

public sealed record AuditDataAuditRow(AuditDataContext Audit);
public sealed record AuditDataElementRow(AuditDataContext Audit, long AuditDocumentElementId, long ElementId, string? ElementTitle, int WeightSnapshot, AuditDocumentElementResult? Result);
public sealed record AuditDataQuestionRow(AuditDataContext Audit, long AuditDocumentElementId, long ElementId, string? ElementTitle, int WeightSnapshot, AuditDocumentElementResult? ElementResult, long AuditQuestionId, long QuestionId, string QuestionText, string? VerificationHint, string? EvidenceHint, AuditQuestionResult? Result, string? Comment, string? Evidence, DateTimeOffset? AnsweredAt, long? AnsweredByUserId, string? AnsweredByDisplayName, long ConcurrencyVersion);
public sealed record AuditDataPage<T>(IReadOnlyList<T> Items, long TotalCount);

[RequiresPermission("Audits.Evaluate")] public sealed record ListAuditDataAuditsQuery(AuditDataFilter Filter, int Skip, int Take, AuditDataAuditSort Sort, bool Descending) : IRequest<Result<AuditDataPage<AuditDataAuditRow>>>;
[RequiresPermission("Audits.Evaluate")] public sealed record ListAuditDataElementsQuery(AuditDataFilter Filter, int Skip, int Take, AuditDataElementSort Sort, bool Descending) : IRequest<Result<AuditDataPage<AuditDataElementRow>>>;
[RequiresPermission("Audits.Evaluate")] public sealed record ListAuditDataQuestionsQuery(AuditDataFilter Filter, int Skip, int Take, AuditDataQuestionSort Sort, bool Descending) : IRequest<Result<AuditDataPage<AuditDataQuestionRow>>>;

public sealed class AuditDataQueryHandler(IAuditariumDbContext db) :
    IRequestHandler<ListAuditDataAuditsQuery, Result<AuditDataPage<AuditDataAuditRow>>>,
    IRequestHandler<ListAuditDataElementsQuery, Result<AuditDataPage<AuditDataElementRow>>>,
    IRequestHandler<ListAuditDataQuestionsQuery, Result<AuditDataPage<AuditDataQuestionRow>>>
{
    public async ValueTask<Result<AuditDataPage<AuditDataAuditRow>>> Handle(ListAuditDataAuditsQuery query, CancellationToken ct)
    {
        if (!Valid(query.Filter, query.Skip, query.Take)) return Invalid<AuditDataPage<AuditDataAuditRow>>();
        var audits = await SelectAuditsAsync(query.Filter, ct);
        var ordered = (query.Sort, query.Descending) switch
        {
            (AuditDataAuditSort.Name, false) => audits.OrderBy(x => x.Audit.AuditName),
            (AuditDataAuditSort.Name, true) => audits.OrderByDescending(x => x.Audit.AuditName),
            (AuditDataAuditSort.State, false) => audits.OrderBy(x => x.Audit.AuditState).ThenBy(x => x.Audit.AuditName),
            (AuditDataAuditSort.State, true) => audits.OrderByDescending(x => x.Audit.AuditState).ThenBy(x => x.Audit.AuditName),
            (AuditDataAuditSort.CreatedAt, false) => audits.OrderBy(x => x.Audit.CreatedAt),
            _ => audits.OrderByDescending(x => x.Audit.CreatedAt)
        };
        return Result<AuditDataPage<AuditDataAuditRow>>.Success(Page(ordered.Select(x => new AuditDataAuditRow(x.Audit)), query.Skip, query.Take));
    }

    public async ValueTask<Result<AuditDataPage<AuditDataElementRow>>> Handle(ListAuditDataElementsQuery query, CancellationToken ct)
    {
        if (!Valid(query.Filter, query.Skip, query.Take)) return Invalid<AuditDataPage<AuditDataElementRow>>();
        var audits = await SelectAuditsAsync(query.Filter, ct);
        var rows = await SelectElementsAsync(audits, ct);
        var ordered = (query.Sort, query.Descending) switch
        {
            (AuditDataElementSort.ElementTitle, false) => rows.OrderBy(x => x.ElementTitle),
            (AuditDataElementSort.ElementTitle, true) => rows.OrderByDescending(x => x.ElementTitle),
            (AuditDataElementSort.Weight, false) => rows.OrderBy(x => x.WeightSnapshot).ThenBy(x => x.ElementTitle),
            (AuditDataElementSort.Weight, true) => rows.OrderByDescending(x => x.WeightSnapshot).ThenBy(x => x.ElementTitle),
            (AuditDataElementSort.CreatedAt, false) => rows.OrderBy(x => x.Audit.CreatedAt),
            _ => rows.OrderByDescending(x => x.Audit.CreatedAt)
        };
        return Result<AuditDataPage<AuditDataElementRow>>.Success(Page(ordered, query.Skip, query.Take));
    }

    public async ValueTask<Result<AuditDataPage<AuditDataQuestionRow>>> Handle(ListAuditDataQuestionsQuery query, CancellationToken ct)
    {
        if (!Valid(query.Filter, query.Skip, query.Take)) return Invalid<AuditDataPage<AuditDataQuestionRow>>();
        var audits = await SelectAuditsAsync(query.Filter, ct);
        var elements = await SelectElementsAsync(audits, ct);
        var elementById = elements.ToDictionary(x => x.AuditDocumentElementId);
        var elementIds = elementById.Keys.ToArray();
        var rows = elementIds.Length == 0 ? [] : await (from question in db.AuditQuestions.AsNoTracking()
                                                        join definition in db.Questions.AsNoTracking() on question.QuestionId equals definition.QuestionId
                                                        join answeredBy in db.Users.AsNoTracking() on question.AnsweredBy equals answeredBy.UserId into answerers
                                                        from answeredBy in answerers.DefaultIfEmpty()
                                                        where elementIds.Contains(question.AuditDocumentElementId)
                                                        select new { question, definition, AnsweredByDisplayName = answeredBy == null ? null : answeredBy.DisplayName }).ToListAsync(ct);
        var projected = rows.Select(x =>
        {
            var element = elementById[x.question.AuditDocumentElementId];
            return new AuditDataQuestionRow(element.Audit, element.AuditDocumentElementId, element.ElementId, element.ElementTitle, element.WeightSnapshot, element.Result, x.question.AuditQuestionId, x.question.QuestionId, x.definition.Text, x.definition.VerificationHint, x.definition.EvidenceHint, x.question.Result, x.question.Comment, x.question.Evidence, x.question.AnsweredAt, x.question.AnsweredBy, x.AnsweredByDisplayName, x.question.ConcurrencyVersion);
        });
        var ordered = (query.Sort, query.Descending) switch
        {
            (AuditDataQuestionSort.QuestionText, false) => projected.OrderBy(x => x.QuestionText),
            (AuditDataQuestionSort.QuestionText, true) => projected.OrderByDescending(x => x.QuestionText),
            (AuditDataQuestionSort.AnsweredAt, false) => projected.OrderBy(x => x.AnsweredAt),
            (AuditDataQuestionSort.AnsweredAt, true) => projected.OrderByDescending(x => x.AnsweredAt),
            (AuditDataQuestionSort.CreatedAt, false) => projected.OrderBy(x => x.Audit.CreatedAt),
            _ => projected.OrderByDescending(x => x.Audit.CreatedAt)
        };
        return Result<AuditDataPage<AuditDataQuestionRow>>.Success(Page(ordered, query.Skip, query.Take));
    }

    private async Task<List<SelectedAudit>> SelectAuditsAsync(AuditDataFilter filter, CancellationToken ct)
    {
        var audits = db.Audits.AsNoTracking();
        if (filter.CreatedFrom is { } from) audits = audits.Where(x => x.CreatedAt >= from);
        if (filter.CreatedTo is { } to) audits = audits.Where(x => x.CreatedAt <= to);
        if (filter.AuditUnitId is { } unitId) audits = audits.Where(x => x.AuditUnitId == unitId);
        if (filter.CatalogVersionId is { } catalogVersionId) audits = audits.Where(x => x.CatalogVersionId == catalogVersionId);
        if (filter.AuditState is { } state) audits = audits.Where(x => x.AuditState == state);

        var joined = await (from audit in audits
                            join unit in db.AuditUnits.IgnoreQueryFilters().AsNoTracking() on audit.AuditUnitId equals unit.AuditUnitId
                            join scope in db.ScopeTypes.AsNoTracking() on unit.ScopeTypeId equals scope.ScopeTypeId
                            join catalog in db.CatalogVersions.AsNoTracking() on audit.CatalogVersionId equals catalog.CatalogVersionId
                            join document in db.Documents.IgnoreQueryFilters().AsNoTracking() on catalog.DocumentId equals document.DocumentId
                            where filter.DocumentId == null || document.DocumentId == filter.DocumentId
                            select new SelectedAuditSource(audit, unit, scope, catalog, document)).ToListAsync(ct);

        return joined.Select(source => SelectedAudit.From(source))
            .Where(x => filter.ScopeTypeId == null || x.Audit.ScopeTypeId == filter.ScopeTypeId)
            .ToList();
    }

    private async Task<List<AuditDataElementRow>> SelectElementsAsync(IReadOnlyList<SelectedAudit> audits, CancellationToken ct)
    {
        var auditById = audits.ToDictionary(x => x.Audit.AuditId);
        var auditIds = auditById.Keys.ToArray();
        if (auditIds.Length == 0) return [];
        var elements = await (from element in db.AuditDocumentElements.AsNoTracking()
                              join definition in db.DocumentElements.AsNoTracking() on element.ElementId equals definition.ElementId
                              where auditIds.Contains(element.AuditId)
                              select new { element, definition.Title }).ToListAsync(ct);
        var elementIds = elements.Select(x => x.element.AuditDocumentElementId).ToArray();
        var questions = elementIds.Length == 0 ? [] : await db.AuditQuestions.AsNoTracking()
            .Where(x => elementIds.Contains(x.AuditDocumentElementId))
            .Select(x => new { x.AuditDocumentElementId, x.Result }).ToListAsync(ct);
        var resultByElement = questions.GroupBy(x => x.AuditDocumentElementId).ToDictionary(x => x.Key, x => ElementResult(x.Select(q => q.Result)));
        return elements.Select(x => new AuditDataElementRow(auditById[x.element.AuditId].Audit, x.element.AuditDocumentElementId, x.element.ElementId, x.Title, x.element.WeightSnapshot, resultByElement.GetValueOrDefault(x.element.AuditDocumentElementId))).ToList();
    }

    private static AuditDocumentElementResult? ElementResult(IEnumerable<AuditQuestionResult?> results)
    {
        var values = results.ToArray();
        if (values.Length == 0 || values.Any(x => x is null)) return null;
        if (values.Contains(AuditQuestionResult.No)) return AuditDocumentElementResult.NotFulfilled;
        if (values.Contains(AuditQuestionResult.NotDeterminable)) return AuditDocumentElementResult.NotDeterminable;
        if (values.All(x => x == AuditQuestionResult.NotApplicable)) return AuditDocumentElementResult.NotApplicable;
        return AuditDocumentElementResult.Fulfilled;
    }

    private static bool Valid(AuditDataFilter filter, int skip, int take) => skip >= 0 && take is >= 1 and <= 200
        && !(filter.CreatedFrom is { } from && filter.CreatedTo is { } to && from > to)
        && new[] { filter.ScopeTypeId, filter.AuditUnitId, filter.DocumentId, filter.CatalogVersionId }.All(x => x is null or > 0);
    private static AuditDataPage<T> Page<T>(IEnumerable<T> items, int skip, int take)
    {
        var materialized = items.ToList();
        return new(materialized.Skip(skip).Take(take).ToList(), materialized.Count);
    }
    private static Result<T> Invalid<T>() => Result<T>.Failure(new AppError("AUDIT_DATA.QUERY_ARGUMENT_INVALID", ErrorType.Validation));

    private sealed record SelectedAudit(AuditDataContext Audit)
    {
        public static SelectedAudit From(SelectedAuditSource source)
        {
            var snapshot = source.Audit.AuditState == AuditState.Draft ? null : DeserializeContext(source.Audit.AuditUnitContext);
            var scopeTypeId = snapshot?.ScopeTypeId ?? source.Unit.ScopeTypeId;
            return new SelectedAudit(new AuditDataContext(source.Audit.AuditId, source.Audit.Name, source.Audit.Description, source.Audit.CreatedAt, source.Audit.AuditState, source.Unit.AuditUnitId,
                snapshot?.Name ?? source.Unit.Name, snapshot?.HierarchyPath, scopeTypeId, source.Scope.Key, source.Scope.Name,
                source.Document.DocumentId, source.Document.Title, source.Document.Version, source.Catalog.CatalogVersionId, source.Catalog.VersionNumber));
        }

        private static AuditUnitContextSnapshot? DeserializeContext(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return JsonSerializer.Deserialize<AuditUnitContextSnapshot>(value);
        }
    }

    private sealed record SelectedAuditSource(Audit Audit, AuditUnit Unit, ScopeType Scope, CatalogVersion Catalog, Document Document);
    private sealed record AuditUnitContextSnapshot([property: JsonPropertyName("name")] string? Name, [property: JsonPropertyName("scope_type_id")] long ScopeTypeId, [property: JsonPropertyName("hierarchy_path")] string? HierarchyPath);
}
