// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Catalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Documents;

[Authorize]
public sealed class CatalogModel(IMediator mediator) : PageModel
{
    public CatalogVersionDetails Catalog { get; private set; } = default!;
    public async Task<IActionResult> OnGetAsync(long id, CancellationToken ct) => await LoadAsync(id, ct);
    public async Task<IActionResult> OnPostReadyAsync(long id, long concurrencyVersion, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new SetCatalogReadyCommand(id, concurrencyVersion, true), ct));
    public async Task<IActionResult> OnPostDraftAsync(long id, long concurrencyVersion, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new SetCatalogDraftCommand(id, concurrencyVersion, true), ct));
    public async Task<IActionResult> OnPostCopyAsync(long id, long concurrencyVersion, CancellationToken ct)
    {
        var result = await mediator.Send(new CopyCatalogVersionCommand(id, concurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage(new { id = result.Value });
        result.ApplyTo(ModelState); return await LoadAsync(id, ct);
    }
    public async Task<IActionResult> OnPostUploadAsync(long id, long concurrencyVersion, IFormFile? sourceFile, CancellationToken ct)
    {
        if (sourceFile is null) { ModelState.AddModelError(string.Empty, "Bitte wählen Sie eine PDF-Datei aus."); return await LoadAsync(id, ct); }
        await using var stream = sourceFile.OpenReadStream();
        return await ExecuteAsync(id, mediator.Send(new ReplaceCatalogOriginalFileCommand(id, concurrencyVersion, sourceFile.FileName, sourceFile.ContentType, stream), ct));
    }
    public async Task<IActionResult> OnPostRemoveAsync(long id, long concurrencyVersion, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new RemoveCatalogOriginalFileCommand(id, concurrencyVersion), ct));
    public async Task<IActionResult> OnGetDownloadAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCatalogOriginalFileQuery(id), ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return await LoadAsync(id, ct); }
        return new FileStreamResult(result.Value!.Content, result.Value.ContentType) { FileDownloadName = result.Value.OriginalFileName, EnableRangeProcessing = true };
    }
    private async Task<IActionResult> ExecuteAsync(long id, ValueTask<Auditarium.Common.Results.Result> pending)
    {
        var result = await pending;
        if (result.IsSuccess) return RedirectToPage(new { id });
        result.ApplyTo(ModelState); return await LoadAsync(id, HttpContext.RequestAborted);
    }
    private async Task<IActionResult> LoadAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCatalogVersionQuery(id), ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        Catalog = result.Value!;
        return Page();
    }
}
