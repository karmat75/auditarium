// SPDX-License-Identifier: MIT
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
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
public enum AuditCsvExportLevel { Audits, DocumentElements, Questions }
public sealed record AuditCsvExport(byte[] Content, string FileName);

[RequiresPermission("Audits.Evaluate")] public sealed record ListAuditDataAuditsQuery(AuditDataFilter Filter, int Skip, int Take, AuditDataAuditSort Sort, bool Descending) : IRequest<Result<AuditDataPage<AuditDataAuditRow>>>;
[RequiresPermission("Audits.Evaluate")] public sealed record ListAuditDataElementsQuery(AuditDataFilter Filter, int Skip, int Take, AuditDataElementSort Sort, bool Descending) : IRequest<Result<AuditDataPage<AuditDataElementRow>>>;
[RequiresPermission("Audits.Evaluate")] public sealed record ListAuditDataQuestionsQuery(AuditDataFilter Filter, int Skip, int Take, AuditDataQuestionSort Sort, bool Descending) : IRequest<Result<AuditDataPage<AuditDataQuestionRow>>>;
[RequiresPermission("Reports.Export")] public sealed record ExportAuditDataCsvQuery(AuditCsvExportLevel Level, AuditDataFilter Filter) : IRequest<Result<AuditCsvExport>>;

public sealed class AuditDataQueryHandler(IAuditariumDbContext db, IAuditEventWriter? auditEvents = null) :
    IRequestHandler<ListAuditDataAuditsQuery, Result<AuditDataPage<AuditDataAuditRow>>>,
    IRequestHandler<ListAuditDataElementsQuery, Result<AuditDataPage<AuditDataElementRow>>>,
    IRequestHandler<ListAuditDataQuestionsQuery, Result<AuditDataPage<AuditDataQuestionRow>>>,
    IRequestHandler<ExportAuditDataCsvQuery, Result<AuditCsvExport>>
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
        var projected = await SelectQuestionsAsync(elements, ct);
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

    public async ValueTask<Result<AuditCsvExport>> Handle(ExportAuditDataCsvQuery query, CancellationToken ct)
    {
        if (!Valid(query.Filter) || !Enum.IsDefined(query.Level)) return Invalid<AuditCsvExport>();

        var audits = await SelectAuditsAsync(query.Filter, ct);
        var csv = query.Level switch
        {
            AuditCsvExportLevel.Audits => Csv(AuditHeader, audits.Select(x => AuditFields(x.Audit))),
            AuditCsvExportLevel.DocumentElements => Csv(ElementHeader, (await SelectElementsAsync(audits, ct)).OrderBy(x => x.Audit.AuditId).ThenBy(x => x.AuditDocumentElementId).Select(ElementFields)),
            AuditCsvExportLevel.Questions => Csv(QuestionHeader, (await SelectQuestionsAsync(await SelectElementsAsync(audits, ct), ct)).OrderBy(x => x.Audit.AuditId).ThenBy(x => x.AuditQuestionId).Select(QuestionFields)),
            _ => throw new InvalidOperationException("Unsupported CSV export level.")
        };

        if (auditEvents is null) throw new InvalidOperationException("CSV export requires an audit-event writer.");
        await auditEvents.WriteAsync(new AuditEvent("EXPORTED", "AuditDataExport", AfterState: new Dictionary<string, object?>
        {
            ["level"] = query.Level.ToString(),
            ["scope_type_id"] = query.Filter.ScopeTypeId,
            ["audit_unit_id"] = query.Filter.AuditUnitId,
            ["document_id"] = query.Filter.DocumentId,
            ["catalog_version_id"] = query.Filter.CatalogVersionId,
            ["audit_state"] = query.Filter.AuditState?.ToString()
        }), ct);

        return Result<AuditCsvExport>.Success(new(csv, $"auditarium-{query.Level.ToString().ToLowerInvariant()}.csv"));
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

    private async Task<IEnumerable<AuditDataQuestionRow>> SelectQuestionsAsync(IReadOnlyList<AuditDataElementRow> elements, CancellationToken ct)
    {
        var elementById = elements.ToDictionary(x => x.AuditDocumentElementId);
        var elementIds = elementById.Keys.ToArray();
        var rows = elementIds.Length == 0 ? [] : await (from question in db.AuditQuestions.AsNoTracking()
                                                        join definition in db.Questions.AsNoTracking() on question.QuestionId equals definition.QuestionId
                                                        join answeredBy in db.Users.AsNoTracking() on question.AnsweredBy equals answeredBy.UserId into answerers
                                                        from answeredBy in answerers.DefaultIfEmpty()
                                                        where elementIds.Contains(question.AuditDocumentElementId)
                                                        select new { question, definition, AnsweredByDisplayName = answeredBy == null ? null : answeredBy.DisplayName }).ToListAsync(ct);
        return rows.Select(x =>
        {
            var element = elementById[x.question.AuditDocumentElementId];
            return new AuditDataQuestionRow(element.Audit, element.AuditDocumentElementId, element.ElementId, element.ElementTitle, element.WeightSnapshot, element.Result, x.question.AuditQuestionId, x.question.QuestionId, x.definition.Text, x.definition.VerificationHint, x.definition.EvidenceHint, x.question.Result, x.question.Comment, x.question.Evidence, x.question.AnsweredAt, x.question.AnsweredBy, x.AnsweredByDisplayName, x.question.ConcurrencyVersion);
        });
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

    private static bool Valid(AuditDataFilter filter, int skip, int take) => skip >= 0 && take is >= 1 and <= 200 && Valid(filter);
    private static bool Valid(AuditDataFilter filter) =>
        !(filter.CreatedFrom is { } from && filter.CreatedTo is { } to && from > to)
        && new[] { filter.ScopeTypeId, filter.AuditUnitId, filter.DocumentId, filter.CatalogVersionId }.All(x => x is null or > 0);
    private static AuditDataPage<T> Page<T>(IEnumerable<T> items, int skip, int take)
    {
        var materialized = items.ToList();
        return new(materialized.Skip(skip).Take(take).ToList(), materialized.Count);
    }
    private static Result<T> Invalid<T>() => Result<T>.Failure(new AppError("AUDIT_DATA.QUERY_ARGUMENT_INVALID", ErrorType.Validation));

    private static readonly string[] AuditHeader = ["audit_id", "audit_name", "audit_description", "audit_date", "audit_state", "audit_unit_id", "audit_unit_name", "audit_unit_hierarchy_path", "audit_unit_scope_type_id", "audit_unit_scope_type_key", "audit_unit_scope_type", "document_id", "document_title", "document_version", "catalog_version_id", "catalog_version"];
    private static readonly string[] ElementHeader = [.. AuditHeader, "audit_document_element_id", "element_id", "element_title", "element_weight", "element_result"];
    private static readonly string[] QuestionHeader = [.. ElementHeader, "audit_question_id", "question_id", "question_text", "verification_hint", "evidence_hint", "result", "comment", "evidence", "answered_at", "answered_by_user_id", "answered_by"];
    private static object?[] AuditFields(AuditDataContext audit) => [audit.AuditId, audit.AuditName, audit.AuditDescription, audit.CreatedAt, audit.AuditState, audit.AuditUnitId, audit.AuditUnitName, audit.AuditUnitHierarchyPath, audit.ScopeTypeId, audit.ScopeTypeKey, audit.ScopeTypeName, audit.DocumentId, audit.DocumentTitle, audit.DocumentVersion, audit.CatalogVersionId, audit.CatalogVersionNumber];
    private static object?[] ElementFields(AuditDataElementRow row) => [.. AuditFields(row.Audit), row.AuditDocumentElementId, row.ElementId, row.ElementTitle, row.WeightSnapshot, row.Result];
    private static object?[] QuestionFields(AuditDataQuestionRow row) => [.. ElementFields(new AuditDataElementRow(row.Audit, row.AuditDocumentElementId, row.ElementId, row.ElementTitle, row.WeightSnapshot, row.ElementResult)), row.AuditQuestionId, row.QuestionId, row.QuestionText, row.VerificationHint, row.EvidenceHint, row.Result, row.Comment, row.Evidence, row.AnsweredAt, row.AnsweredByUserId, row.AnsweredByDisplayName];
    private static byte[] Csv(IEnumerable<string> header, IEnumerable<object?[]> rows)
    {
        var output = new StringBuilder();
        WriteRow(output, header); foreach (var row in rows) WriteRow(output, row);
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(output.ToString());
    }
    private static void WriteRow(StringBuilder output, IEnumerable<object?> values) => output.AppendJoin(',', values.Select(Escape)).Append("\r\n");
    private static string Escape(object? value)
    {
        var text = value switch { null => string.Empty, DateTimeOffset timestamp => timestamp.ToString("O", global::System.Globalization.CultureInfo.InvariantCulture), _ => Convert.ToString(value, global::System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty };
        return '"' + text.Replace("\"", "\"\"") + '"';
    }

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
