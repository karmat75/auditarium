// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Auditarium.Api;

public static class AuditDataEndpoints
{
    public static IEndpointRouteBuilder MapAuditDataEndpoints(this IEndpointRouteBuilder routes)
    {
        var data = routes.MapGroup("/api/v1/audit-data").RequireAuthorization();
        data.MapGet("/audits", ListAudits).WithName("ListAuditDataAudits").Produces<AuditDataPageResponse<AuditDataAuditContract>>().Produces<ProblemDetails>(400).Produces<ProblemDetails>(403);
        data.MapGet("/document-elements", ListElements).WithName("ListAuditDataElements").Produces<AuditDataPageResponse<AuditDataElementContract>>().Produces<ProblemDetails>(400).Produces<ProblemDetails>(403);
        data.MapGet("/questions", ListQuestions).WithName("ListAuditDataQuestions").Produces<AuditDataPageResponse<AuditDataQuestionContract>>().Produces<ProblemDetails>(400).Produces<ProblemDetails>(403);
        return routes;
    }

    private static async Task<IResult> ListAudits(IMediator mediator, [AsParameters] AuditDataFilterContract filter, int page = 1, int pageSize = 50, string? sort = null, CancellationToken ct = default)
    {
        var paging = new PageRequest(page, pageSize).Validate(); if (!paging.IsSuccess) return ApiProblemDetails.From(paging);
        if (!TrySort(sort, new Dictionary<string, AuditDataAuditSort>(StringComparer.Ordinal) { ["createdAt"] = AuditDataAuditSort.CreatedAt, ["name"] = AuditDataAuditSort.Name, ["state"] = AuditDataAuditSort.State }, AuditDataAuditSort.CreatedAt, out var order, out var invalid)) return invalid!;
        var result = await mediator.Send(new ListAuditDataAuditsQuery(filter.ToBll(), (page - 1) * pageSize, pageSize, order.Value, order.Descending), ct);
        return result.IsSuccess ? Results.Ok(new AuditDataPageResponse<AuditDataAuditContract>(result.Value!.Items.Select(x => new AuditDataAuditContract(AuditDataContextContract.From(x.Audit))).ToList(), page, pageSize, result.Value.TotalCount)) : ApiProblemDetails.From(result);
    }

    private static async Task<IResult> ListElements(IMediator mediator, [AsParameters] AuditDataFilterContract filter, int page = 1, int pageSize = 50, string? sort = null, CancellationToken ct = default)
    {
        var paging = new PageRequest(page, pageSize).Validate(); if (!paging.IsSuccess) return ApiProblemDetails.From(paging);
        if (!TrySort(sort, new Dictionary<string, AuditDataElementSort>(StringComparer.Ordinal) { ["createdAt"] = AuditDataElementSort.CreatedAt, ["elementTitle"] = AuditDataElementSort.ElementTitle, ["weight"] = AuditDataElementSort.Weight }, AuditDataElementSort.CreatedAt, out var order, out var invalid)) return invalid!;
        var result = await mediator.Send(new ListAuditDataElementsQuery(filter.ToBll(), (page - 1) * pageSize, pageSize, order.Value, order.Descending), ct);
        return result.IsSuccess ? Results.Ok(new AuditDataPageResponse<AuditDataElementContract>(result.Value!.Items.Select(AuditDataElementContract.From).ToList(), page, pageSize, result.Value.TotalCount)) : ApiProblemDetails.From(result);
    }

    private static async Task<IResult> ListQuestions(IMediator mediator, [AsParameters] AuditDataFilterContract filter, int page = 1, int pageSize = 50, string? sort = null, CancellationToken ct = default)
    {
        var paging = new PageRequest(page, pageSize).Validate(); if (!paging.IsSuccess) return ApiProblemDetails.From(paging);
        if (!TrySort(sort, new Dictionary<string, AuditDataQuestionSort>(StringComparer.Ordinal) { ["createdAt"] = AuditDataQuestionSort.CreatedAt, ["questionText"] = AuditDataQuestionSort.QuestionText, ["answeredAt"] = AuditDataQuestionSort.AnsweredAt }, AuditDataQuestionSort.CreatedAt, out var order, out var invalid)) return invalid!;
        var result = await mediator.Send(new ListAuditDataQuestionsQuery(filter.ToBll(), (page - 1) * pageSize, pageSize, order.Value, order.Descending), ct);
        return result.IsSuccess ? Results.Ok(new AuditDataPageResponse<AuditDataQuestionContract>(result.Value!.Items.Select(AuditDataQuestionContract.From).ToList(), page, pageSize, result.Value.TotalCount)) : ApiProblemDetails.From(result);
    }

    private static bool TrySort<T>(string? sort, IReadOnlyDictionary<string, T> allowed, T defaultSort, out (T Value, bool Descending) order, out IResult? invalid)
    {
        var parsed = SortRequest.Parse(sort, allowed.Keys.ToHashSet(StringComparer.Ordinal));
        if (!parsed.IsSuccess) { order = default; invalid = ApiProblemDetails.From(parsed); return false; }
        order = parsed.Value is { } value ? (allowed[value.Field], value.Direction == SortDirection.Descending) : (defaultSort, true);
        invalid = null;
        return true;
    }
}

public sealed record AuditDataFilterContract(DateTimeOffset? CreatedFrom = null, DateTimeOffset? CreatedTo = null, long? ScopeTypeId = null, long? AuditUnitId = null, long? DocumentId = null, long? CatalogVersionId = null, AuditState? AuditState = null)
{
    public AuditDataFilter ToBll() => new(CreatedFrom, CreatedTo, ScopeTypeId, AuditUnitId, DocumentId, CatalogVersionId, AuditState);
}

public sealed record AuditDataPageResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount);
public sealed record AuditDataContextContract(long AuditId, string AuditName, string? AuditDescription, DateTimeOffset CreatedAt, AuditState AuditState, long AuditUnitId, string AuditUnitName, string? AuditUnitHierarchyPath, long ScopeTypeId, string ScopeTypeKey, string ScopeTypeName, long DocumentId, string DocumentTitle, string? DocumentVersion, long CatalogVersionId, int CatalogVersionNumber)
{
    public static AuditDataContextContract From(AuditDataContext value) => new(value.AuditId, value.AuditName, value.AuditDescription, value.CreatedAt, value.AuditState, value.AuditUnitId, value.AuditUnitName, value.AuditUnitHierarchyPath, value.ScopeTypeId, value.ScopeTypeKey, value.ScopeTypeName, value.DocumentId, value.DocumentTitle, value.DocumentVersion, value.CatalogVersionId, value.CatalogVersionNumber);
}
public sealed record AuditDataAuditContract(AuditDataContextContract Audit);
public sealed record AuditDataElementContract(AuditDataContextContract Audit, long AuditDocumentElementId, long ElementId, string? ElementTitle, int WeightSnapshot, AuditDocumentElementResult? Result)
{
    public static AuditDataElementContract From(AuditDataElementRow value) => new(AuditDataContextContract.From(value.Audit), value.AuditDocumentElementId, value.ElementId, value.ElementTitle, value.WeightSnapshot, value.Result);
}
public sealed record AuditDataQuestionContract(AuditDataContextContract Audit, long AuditDocumentElementId, long ElementId, string? ElementTitle, int WeightSnapshot, AuditDocumentElementResult? ElementResult, long AuditQuestionId, long QuestionId, string QuestionText, string? VerificationHint, string? EvidenceHint, AuditQuestionResult? Result, string? Comment, string? Evidence, DateTimeOffset? AnsweredAt, long? AnsweredByUserId, string? AnsweredByDisplayName, long ConcurrencyVersion)
{
    public static AuditDataQuestionContract From(AuditDataQuestionRow value) => new(AuditDataContextContract.From(value.Audit), value.AuditDocumentElementId, value.ElementId, value.ElementTitle, value.WeightSnapshot, value.ElementResult, value.AuditQuestionId, value.QuestionId, value.QuestionText, value.VerificationHint, value.EvidenceHint, value.Result, value.Comment, value.Evidence, value.AnsweredAt, value.AnsweredByUserId, value.AnsweredByDisplayName, value.ConcurrencyVersion);
}
