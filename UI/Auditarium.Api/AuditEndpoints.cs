// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Auditarium.Api;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder routes)
    {
        var units = routes.MapGroup("/api/v1/audit-units").RequireAuthorization();
        units.MapGet("/", ListAuditUnits).WithName("ListAuditUnits").Produces<PageResponse<AuditUnitListItem>>().Produces<ProblemDetails>(400);
        units.MapGet("/hierarchy", GetAuditUnitHierarchy).WithName("GetAuditUnitHierarchy").Produces<IReadOnlyList<AuditUnitHierarchyItem>>();
        units.MapGet("/form-options", GetAuditUnitFormOptions).WithName("GetAuditUnitFormOptions").Produces<AuditUnitFormOptions>();
        units.MapPost("/", CreateAuditUnit).WithName("CreateAuditUnit").Produces<long>(201).Produces<ProblemDetails>(400);
        units.MapGet("/{auditUnitId:long}", GetAuditUnit).WithName("GetAuditUnit").Produces<AuditUnitDetails>().Produces<ProblemDetails>(404);
        units.MapPut("/{auditUnitId:long}", UpdateAuditUnit).WithName("UpdateAuditUnit").Produces(204).Produces<ProblemDetails>(409);

        var audits = routes.MapGroup("/api/v1/audits").RequireAuthorization();
        audits.MapGet("/", ListAudits).WithName("ListAudits").Produces<PageResponse<AuditListItem>>().Produces<ProblemDetails>(400);
        audits.MapGet("/configuration-options", GetConfigurationOptions).WithName("GetAuditConfigurationOptions").Produces<AuditConfigurationOptions>();
        audits.MapPost("/", CreateAudit).WithName("CreateAudit").Produces<long>(201).Produces<ProblemDetails>(400);
        audits.MapGet("/{auditId:long}", GetAudit).WithName("GetAudit").Produces<AuditDetails>().Produces<ProblemDetails>(404);
        audits.MapPut("/{auditId:long}", UpdateAuditDraft).WithName("UpdateAuditDraft").Produces(204).Produces<ProblemDetails>(409);
        audits.MapGet("/{auditId:long}/preview", GetPreview).WithName("GetAuditPreview").Produces<AuditPreview>();
        audits.MapPost("/{auditId:long}/publish", Publish).WithName("PublishAudit").Produces(204).Produces<ProblemDetails>(409);
        audits.MapGet("/auditor-options", GetAuditorOptions).WithName("GetAuditAuditorOptions").Produces<IReadOnlyList<AuditorOption>>().Produces<ProblemDetails>(403);
        audits.MapPost("/{auditId:long}/claim", Claim).WithName("ClaimAudit").Produces(204).Produces<ProblemDetails>(409);
        audits.MapPost("/{auditId:long}/release-own", ReleaseOwn).WithName("ReleaseOwnAudit").Produces(204).Produces<ProblemDetails>(409);
        audits.MapPost("/{auditId:long}/assignment", Assign).WithName("AssignAuditAuditor").Produces(204).Produces<ProblemDetails>(409);
        audits.MapPost("/{auditId:long}/finalize", FinalizeAuditAction).WithName("FinalizeAudit").Produces(204).Produces<ProblemDetails>(409);
        audits.MapPost("/{auditId:long}/cancel", Cancel).WithName("CancelAudit").Produces(204).Produces<ProblemDetails>(400).Produces<ProblemDetails>(409);
        audits.MapPost("/{auditId:long}/reopen", Reopen).WithName("ReopenAudit").Produces(204).Produces<ProblemDetails>(400).Produces<ProblemDetails>(409);
        audits.MapPut("/{auditId:long}/questions/{auditQuestionId:long}", AnswerQuestion).WithName("AnswerAuditQuestion").Produces(204).Produces<ProblemDetails>(400).Produces<ProblemDetails>(409);
        audits.MapPost("/{auditId:long}/questions/{auditQuestionId:long}/reset", ResetQuestion).WithName("ResetAuditQuestion").Produces(204).Produces<ProblemDetails>(409);
        return routes;
    }

    private static async Task<IResult> ListAuditUnits(IMediator mediator, string? search, int page = 1, int pageSize = 50, string? sort = null, CancellationToken ct = default)
    {
        var paging = new PageRequest(page, pageSize).Validate(); if (!paging.IsSuccess) return ApiProblemDetails.From(paging);
        var parsedSort = SortRequest.Parse(sort, new HashSet<string>(StringComparer.Ordinal) { "name", "scopeType", "usageState" }); if (!parsedSort.IsSuccess) return ApiProblemDetails.From(parsedSort);
        var order = parsedSort.Value ?? new SortRequest("name", SortDirection.Ascending);
        var result = await mediator.Send(new ListAuditUnitsQuery(search, (page - 1) * pageSize, pageSize, order.Field, order.Direction == SortDirection.Descending), ct);
        return result.IsSuccess ? Results.Ok(new PageResponse<AuditUnitListItem>(result.Value!.Items, page, pageSize, result.Value.TotalCount)) : ApiProblemDetails.From(result);
    }

    private static Task<IResult> GetAuditUnitHierarchy(IMediator mediator, CancellationToken ct) => Send(mediator.Send(new GetAuditUnitHierarchyQuery(), ct));
    private static Task<IResult> GetAuditUnitFormOptions(IMediator mediator, CancellationToken ct) => Send(mediator.Send(new GetAuditUnitFormOptionsQuery(), ct));
    private static Task<IResult> GetAuditUnit(IMediator mediator, long auditUnitId, CancellationToken ct) => Send(mediator.Send(new GetAuditUnitQuery(auditUnitId), ct));
    private static async Task<IResult> CreateAuditUnit(IMediator mediator, AuditUnitContract contract, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateAuditUnitCommand(contract.ToInput()), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/audit-units/{result.Value}", result.Value) : ApiProblemDetails.From(result);
    }
    private static Task<IResult> UpdateAuditUnit(IMediator mediator, long auditUnitId, AuditUnitContract contract, CancellationToken ct) => Send(mediator.Send(new UpdateAuditUnitCommand(auditUnitId, contract.ToInput(), contract.ConcurrencyVersion), ct));

    private static async Task<IResult> ListAudits(IMediator mediator, string? search, AuditState? state, int page = 1, int pageSize = 50, string? sort = null, CancellationToken ct = default)
    {
        var paging = new PageRequest(page, pageSize).Validate(); if (!paging.IsSuccess) return ApiProblemDetails.From(paging);
        var parsedSort = SortRequest.Parse(sort, new HashSet<string>(StringComparer.Ordinal) { "name", "createdAt", "state" }); if (!parsedSort.IsSuccess) return ApiProblemDetails.From(parsedSort);
        var order = parsedSort.Value ?? new SortRequest("createdAt", SortDirection.Descending);
        var result = await mediator.Send(new ListAuditsQuery(search, state, (page - 1) * pageSize, pageSize, order.Field, order.Direction == SortDirection.Descending), ct);
        return result.IsSuccess ? Results.Ok(new PageResponse<AuditListItem>(result.Value!.Items, page, pageSize, result.Value.TotalCount)) : ApiProblemDetails.From(result);
    }
    private static Task<IResult> GetConfigurationOptions(IMediator mediator, CancellationToken ct) => Send(mediator.Send(new GetAuditConfigurationOptionsQuery(), ct));
    private static async Task<IResult> CreateAudit(IMediator mediator, AuditContract contract, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateAuditCommand(contract.ToInput()), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/audits/{result.Value}", result.Value) : ApiProblemDetails.From(result);
    }
    private static Task<IResult> GetAudit(IMediator mediator, long auditId, CancellationToken ct) => Send(mediator.Send(new GetAuditQuery(auditId), ct));
    private static Task<IResult> UpdateAuditDraft(IMediator mediator, long auditId, AuditContract contract, CancellationToken ct) => Send(mediator.Send(new UpdateAuditDraftCommand(auditId, contract.ToInput(), contract.ConcurrencyVersion), ct));
    private static Task<IResult> GetPreview(IMediator mediator, long auditId, CancellationToken ct) => Send(mediator.Send(new GetAuditPreviewQuery(auditId), ct));
    private static Task<IResult> Publish(IMediator mediator, long auditId, ConcurrencyContract contract, CancellationToken ct) => Send(mediator.Send(new PublishAuditCommand(auditId, contract.ConcurrencyVersion), ct));
    private static Task<IResult> GetAuditorOptions(IMediator mediator, CancellationToken ct) => Send(mediator.Send(new GetAuditorOptionsQuery(), ct));
    private static Task<IResult> Claim(IMediator mediator, long auditId, ConcurrencyContract contract, CancellationToken ct) => Send(mediator.Send(new ClaimAuditCommand(auditId, contract.ConcurrencyVersion), ct));
    private static Task<IResult> ReleaseOwn(IMediator mediator, long auditId, ConcurrencyContract contract, CancellationToken ct) => Send(mediator.Send(new ReleaseOwnAuditCommand(auditId, contract.ConcurrencyVersion), ct));
    private static Task<IResult> Assign(IMediator mediator, long auditId, AssignmentContract contract, CancellationToken ct) => Send(mediator.Send(new AssignAuditorCommand(auditId, contract.UserId, contract.ConcurrencyVersion), ct));
    private static Task<IResult> FinalizeAuditAction(IMediator mediator, long auditId, ConcurrencyContract contract, CancellationToken ct) => Send(mediator.Send(new FinalizeAuditCommand(auditId, contract.ConcurrencyVersion), ct));
    private static Task<IResult> Cancel(IMediator mediator, long auditId, ReasonContract contract, CancellationToken ct) => Send(mediator.Send(new CancelAuditCommand(auditId, contract.Reason, contract.ConcurrencyVersion), ct));
    private static Task<IResult> Reopen(IMediator mediator, long auditId, ReasonContract contract, CancellationToken ct) => Send(mediator.Send(new ReopenAuditCommand(auditId, contract.Reason, contract.ConcurrencyVersion), ct));
    private static Task<IResult> AnswerQuestion(IMediator mediator, long auditId, long auditQuestionId, AnswerAuditQuestionContract contract, CancellationToken ct) => Send(mediator.Send(new AnswerAuditQuestionCommand(auditId, auditQuestionId, contract.Result, contract.Comment, contract.Evidence, contract.ConcurrencyVersion), ct));
    private static Task<IResult> ResetQuestion(IMediator mediator, long auditId, long auditQuestionId, ConcurrencyContract contract, CancellationToken ct) => Send(mediator.Send(new ResetAuditQuestionCommand(auditId, auditQuestionId, contract.ConcurrencyVersion), ct));
    private static async Task<IResult> Send(ValueTask<Auditarium.Common.Results.Result> pending) => ApiProblemDetails.From(await pending);
    private static async Task<IResult> Send<T>(ValueTask<Auditarium.Common.Results.Result<T>> pending) => ApiProblemDetails.From(await pending);
}

public sealed record AuditUnitContract(long? ParentAuditUnitId, long ScopeTypeId, string Name, string? Description, AuditUnitUsageState UsageState, string? UsageStateReason, string? Notes, long ConcurrencyVersion = 0)
{ public AuditUnitInput ToInput() => new(ParentAuditUnitId, ScopeTypeId, Name, Description, UsageState, UsageStateReason, Notes); }

public sealed record AuditSettingsContract(bool YesCommentRequired = false, bool YesEvidenceRequired = false, bool NoCommentRequired = false, bool NoEvidenceRequired = false, bool NotApplicableCommentRequired = true, bool NotApplicableEvidenceRequired = false, bool NotDeterminableCommentRequired = true, bool NotDeterminableEvidenceRequired = false)
{
    public AuditSettings ToBll() => new(new Dictionary<AuditQuestionResult, ResponseRule>
    {
        [AuditQuestionResult.Yes] = new(YesCommentRequired, YesEvidenceRequired),
        [AuditQuestionResult.No] = new(NoCommentRequired, NoEvidenceRequired),
        [AuditQuestionResult.NotApplicable] = new(NotApplicableCommentRequired, NotApplicableEvidenceRequired),
        [AuditQuestionResult.NotDeterminable] = new(NotDeterminableCommentRequired, NotDeterminableEvidenceRequired)
    });
}

public sealed record AuditContract(string Name, string? Description, long AuditUnitId, long CatalogVersionId, AuditSettingsContract? Settings, string? Notes, long ConcurrencyVersion = 0)
{ public AuditInput ToInput() => new(Name, Description, AuditUnitId, CatalogVersionId, Settings?.ToBll(), Notes); }
public sealed record AssignmentContract(long? UserId, long ConcurrencyVersion);
public sealed record ReasonContract(string Reason, long ConcurrencyVersion);
public sealed record AnswerAuditQuestionContract(AuditQuestionResult Result, string? Comment, string? Evidence, long ConcurrencyVersion);
