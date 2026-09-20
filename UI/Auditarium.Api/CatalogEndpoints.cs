// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Catalog;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Auditarium.Api;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder routes)
    {
        var documents = routes.MapGroup("/api/v1/documents").RequireAuthorization();
        documents.MapGet("/", ListDocuments).WithName("ListDocuments").Produces<PageResponse<DocumentListItem>>().Produces<ProblemDetails>(400);
        documents.MapPost("/", CreateDocument).WithName("CreateDocument").Produces<long>(201).Produces<ProblemDetails>(400);
        documents.MapGet("/{documentId:long}", GetDocument).WithName("GetDocument").Produces<DocumentDetails>().Produces<ProblemDetails>(404);
        documents.MapPut("/{documentId:long}", UpdateDocument).WithName("UpdateDocument").Produces(204).Produces<ProblemDetails>(409);
        documents.MapPost("/{documentId:long}/catalog-versions", CreateCatalogVersion).WithName("CreateCatalogVersion").Produces<long>(201);
        documents.MapGet("/{documentId:long}/catalog-versions", ListCatalogVersions).WithName("ListCatalogVersions").Produces<IReadOnlyList<CatalogVersionListItem>>();
        var catalogs = routes.MapGroup("/api/v1/catalog-versions").RequireAuthorization();
        catalogs.MapGet("/{catalogVersionId:long}", GetCatalogVersion).WithName("GetCatalogVersion").Produces<CatalogVersionDetails>();
        catalogs.MapPost("/{catalogVersionId:long}/copy", CopyCatalogVersion).WithName("CopyCatalogVersion").Produces<long>(201);
        catalogs.MapPost("/{catalogVersionId:long}/ready", SetReady).WithName("SetCatalogReady").Produces(204);
        catalogs.MapPost("/{catalogVersionId:long}/draft", SetDraft).WithName("SetCatalogDraft").Produces(204);
        catalogs.MapPut("/{catalogVersionId:long}/source-file", ReplaceSourceFile).WithName("ReplaceCatalogSourceFile").Accepts<IFormFile>("multipart/form-data").Produces(204);
        catalogs.MapDelete("/{catalogVersionId:long}/source-file", RemoveSourceFile).WithName("RemoveCatalogSourceFile").Produces(204);
        catalogs.MapGet("/{catalogVersionId:long}/source-file", DownloadSourceFile).WithName("DownloadCatalogSourceFile").Produces(200, contentType: "application/pdf");
        return routes;
    }

    private static async Task<IResult> ListDocuments(IMediator mediator, string? search, int page = 1, int pageSize = 50, string? sort = null, CancellationToken ct = default)
    {
        var paging = new PageRequest(page, pageSize).Validate(); if (!paging.IsSuccess) return ApiProblemDetails.From(paging);
        var parsedSort = SortRequest.Parse(sort, new HashSet<string>(StringComparer.Ordinal) { "title", "publisher", "version", "usageState" }); if (!parsedSort.IsSuccess) return ApiProblemDetails.From(parsedSort);
        var order = parsedSort.Value ?? new SortRequest("title", SortDirection.Ascending);
        var result = await mediator.Send(new ListDocumentsQuery(search, (page - 1) * pageSize, pageSize, order.Field, order.Direction == SortDirection.Descending), ct);
        return result.IsSuccess ? Results.Ok(new PageResponse<DocumentListItem>(result.Value!.Items, page, pageSize, result.Value.TotalCount)) : ApiProblemDetails.From(result);
    }
    private static async Task<IResult> GetDocument(IMediator mediator, long documentId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new GetDocumentQuery(documentId), ct));
    private static async Task<IResult> CreateDocument(IMediator mediator, DocumentContract contract, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateDocumentCommand(contract.ToInput()), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/documents/{result.Value}", result.Value) : ApiProblemDetails.From(result);
    }
    private static async Task<IResult> UpdateDocument(IMediator mediator, long documentId, DocumentContract contract, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new UpdateDocumentCommand(documentId, contract.ToInput(), contract.ConcurrencyVersion), ct));
    private static async Task<IResult> CreateCatalogVersion(IMediator mediator, long documentId, CreateCatalogVersionContract contract, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateCatalogVersionCommand(documentId, contract.Notes), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/catalog-versions/{result.Value}", result.Value) : ApiProblemDetails.From(result);
    }
    private static async Task<IResult> ListCatalogVersions(IMediator mediator, long documentId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new ListCatalogVersionsQuery(documentId), ct));
    private static async Task<IResult> GetCatalogVersion(IMediator mediator, long catalogVersionId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new GetCatalogVersionQuery(catalogVersionId), ct));
    private static async Task<IResult> CopyCatalogVersion(IMediator mediator, long catalogVersionId, ConcurrencyContract contract, CancellationToken ct)
    {
        var result = await mediator.Send(new CopyCatalogVersionCommand(catalogVersionId, contract.ConcurrencyVersion), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/catalog-versions/{result.Value}", result.Value) : ApiProblemDetails.From(result);
    }
    private static async Task<IResult> SetReady(IMediator mediator, long catalogVersionId, ConfirmCatalogStateContract contract, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new SetCatalogReadyCommand(catalogVersionId, contract.ConcurrencyVersion, contract.Confirmed), ct));
    private static async Task<IResult> SetDraft(IMediator mediator, long catalogVersionId, ConfirmCatalogStateContract contract, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new SetCatalogDraftCommand(catalogVersionId, contract.ConcurrencyVersion, contract.Confirmed), ct));
    private static async Task<IResult> ReplaceSourceFile(IMediator mediator, long catalogVersionId, IFormFile file, [FromForm] long concurrencyVersion, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return ApiProblemDetails.From(await mediator.Send(new ReplaceCatalogOriginalFileCommand(catalogVersionId, concurrencyVersion, file.FileName, file.ContentType, stream), ct));
    }
    private static async Task<IResult> RemoveSourceFile(IMediator mediator, long catalogVersionId, long concurrencyVersion, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new RemoveCatalogOriginalFileCommand(catalogVersionId, concurrencyVersion), ct));
    private static async Task<IResult> DownloadSourceFile(IMediator mediator, long catalogVersionId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCatalogOriginalFileQuery(catalogVersionId), ct);
        return result.IsSuccess ? Results.File(result.Value!.Content, result.Value.ContentType, result.Value.OriginalFileName, enableRangeProcessing: true) : ApiProblemDetails.From(result);
    }
}

public sealed record DocumentContract(string Title, string? Publisher, string? Version, DateOnly? PublicationDate, string? Source, DocumentUsageState UsageState, string? UsageStateReason, string? Notes, long ConcurrencyVersion = 0)
{ public DocumentInput ToInput() => new(Title, Publisher, Version, PublicationDate, Source, UsageState, UsageStateReason, Notes); }
public sealed record CreateCatalogVersionContract(string? Notes);
public sealed record ConcurrencyContract(long ConcurrencyVersion);
public sealed record ConfirmCatalogStateContract(long ConcurrencyVersion, bool Confirmed);
