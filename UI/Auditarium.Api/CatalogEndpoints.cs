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
        catalogs.MapGet("/{catalogVersionId:long}/editor", GetCatalogEditor).WithName("GetCatalogEditor").Produces<CatalogEditorDetails>();
        catalogs.MapPost("/{catalogVersionId:long}/elements", AddElement).WithName("AddDocumentElement").Produces<long>(201);
        catalogs.MapPut("/elements/{elementId:long}", UpdateElement).WithName("UpdateDocumentElement").Produces(204);
        catalogs.MapPost("/elements/{elementId:long}/move", MoveElement).WithName("MoveDocumentElement").Produces(204);
        catalogs.MapGet("/elements/{elementId:long}/delete-preview", DeleteElementPreview).WithName("GetDeleteDocumentElementPreview").Produces<DeleteDocumentElementPreview>();
        catalogs.MapDelete("/elements/{elementId:long}", DeleteElement).WithName("DeleteDocumentElement").Produces(204);
        catalogs.MapPost("/elements/{elementId:long}/questions", AddQuestion).WithName("AddQuestion").Produces<long>(201);
        catalogs.MapPut("/questions/{questionId:long}", UpdateQuestion).WithName("UpdateQuestion").Produces(204);
        catalogs.MapPost("/questions/{questionId:long}/move", MoveQuestion).WithName("MoveQuestion").Produces(204);
        catalogs.MapDelete("/questions/{questionId:long}", DeleteQuestion).WithName("DeleteQuestion").Produces(204);
        catalogs.MapPut("/elements/{elementId:long}/weight", SetWeight).WithName("SetDocumentElementWeight").Produces(204);
        catalogs.MapPost("/imports/validate", ValidateImport).WithName("ValidateCatalogImport").Produces<CatalogImportReport>();
        catalogs.MapPost("/imports/apply", ApplyImport).WithName("ApplyCatalogImport").Produces<CatalogImportReport>();
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
    private static async Task<IResult> GetCatalogEditor(IMediator mediator, long catalogVersionId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new GetCatalogEditorQuery(catalogVersionId), ct));
    private static async Task<IResult> AddElement(IMediator mediator, long catalogVersionId, ElementContract contract, CancellationToken ct)
    { var result = await mediator.Send(new AddDocumentElementCommand(catalogVersionId, contract.ParentElementId, contract.Title, contract.Text, contract.Notes), ct); return result.IsSuccess ? Results.Created($"/api/v1/catalog-versions/elements/{result.Value}", result.Value) : ApiProblemDetails.From(result); }
    private static Task<IResult> UpdateElement(IMediator mediator, long elementId, ElementContract contract, CancellationToken ct) => Send(mediator.Send(new UpdateDocumentElementCommand(elementId, contract.Title, contract.Text, contract.Notes), ct));
    private static Task<IResult> MoveElement(IMediator mediator, long elementId, MoveElementContract contract, CancellationToken ct) => Send(mediator.Send(new MoveDocumentElementCommand(elementId, contract.ParentElementId, contract.SortOrder), ct));
    private static Task<IResult> DeleteElementPreview(IMediator mediator, long elementId, CancellationToken ct) => Send(mediator.Send(new GetDeleteDocumentElementPreviewQuery(elementId), ct));
    private static Task<IResult> DeleteElement(IMediator mediator, long elementId, ConfirmContract contract, CancellationToken ct) => Send(mediator.Send(new DeleteDocumentElementCommand(elementId, contract.Confirmed), ct));
    private static async Task<IResult> AddQuestion(IMediator mediator, long elementId, QuestionContract contract, CancellationToken ct)
    { var result = await mediator.Send(new AddQuestionCommand(elementId, contract.Text, contract.VerificationHint, contract.EvidenceHint, contract.Notes, contract.ScopeTypeIds), ct); return result.IsSuccess ? Results.Created($"/api/v1/catalog-versions/questions/{result.Value}", result.Value) : ApiProblemDetails.From(result); }
    private static Task<IResult> UpdateQuestion(IMediator mediator, long questionId, QuestionContract contract, CancellationToken ct) => Send(mediator.Send(new UpdateQuestionCommand(questionId, contract.Text, contract.VerificationHint, contract.EvidenceHint, contract.Notes, contract.ScopeTypeIds), ct));
    private static Task<IResult> MoveQuestion(IMediator mediator, long questionId, MoveQuestionContract contract, CancellationToken ct) => Send(mediator.Send(new MoveQuestionCommand(questionId, contract.SortOrder), ct));
    private static Task<IResult> DeleteQuestion(IMediator mediator, long questionId, ConfirmContract contract, CancellationToken ct) => Send(mediator.Send(new DeleteQuestionCommand(questionId, contract.Confirmed), ct));
    private static Task<IResult> SetWeight(IMediator mediator, long elementId, WeightContract contract, CancellationToken ct) => Send(mediator.Send(new SetDocumentElementWeightCommand(elementId, contract.Weight), ct));
    private static Task<IResult> ValidateImport(IMediator mediator, ImportValidateContract contract, CancellationToken ct) => Send(mediator.Send(new ValidateCatalogImportCommand(contract.PackageJson), ct));
    private static Task<IResult> ApplyImport(IMediator mediator, ImportApplyContract contract, CancellationToken ct) => Send(mediator.Send(new ApplyCatalogImportCommand(contract.PackageJson, contract.Mode, contract.SelectedRootElementIds), ct));
    private static async Task<IResult> Send(ValueTask<Auditarium.Common.Results.Result> pending) => ApiProblemDetails.From(await pending);
    private static async Task<IResult> Send<T>(ValueTask<Auditarium.Common.Results.Result<T>> pending) => ApiProblemDetails.From(await pending);
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
public sealed record ElementContract(long? ParentElementId, string? Title, string? Text, string? Notes);
public sealed record MoveElementContract(long? ParentElementId, int SortOrder);
public sealed record QuestionContract(string Text, string? VerificationHint, string? EvidenceHint, string? Notes, IReadOnlyCollection<long> ScopeTypeIds);
public sealed record MoveQuestionContract(int SortOrder);
public sealed record WeightContract(int Weight);
public sealed record ConfirmContract(bool Confirmed);
public sealed record ImportValidateContract(string PackageJson);
public sealed record ImportApplyContract(string PackageJson, ImportApplyMode Mode, IReadOnlyCollection<string> SelectedRootElementIds);
