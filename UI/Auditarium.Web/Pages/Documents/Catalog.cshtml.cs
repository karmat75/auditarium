// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Catalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Documents;

public sealed class ElementInputModel { public long ElementId { get; set; } public long? ParentElementId { get; set; } public int SortOrder { get; set; } public string? Title { get; set; } public string? Text { get; set; } public string? Notes { get; set; } }
public sealed class QuestionInputModel { public long QuestionId { get; set; } public long ElementId { get; set; } public int SortOrder { get; set; } public string Text { get; set; } = string.Empty; public string? VerificationHint { get; set; } public string? EvidenceHint { get; set; } public string? Notes { get; set; } public long[] ScopeTypeIds { get; set; } = []; }
public sealed class WeightInputModel { public long ElementId { get; set; } public int Weight { get; set; } = 3; }
public sealed class ImportInputModel { public string PackageJson { get; set; } = string.Empty; public ImportApplyMode Mode { get; set; } = ImportApplyMode.Full; public string[] SelectedRootElementIds { get; set; } = []; }

[Authorize]
public sealed class CatalogModel(IMediator mediator) : PageModel
{
    [BindProperty] public ElementInputModel ElementInput { get; set; } = new();
    [BindProperty] public QuestionInputModel QuestionInput { get; set; } = new();
    [BindProperty] public WeightInputModel WeightInput { get; set; } = new();
    [BindProperty] public ImportInputModel ImportInput { get; set; } = new();
    public CatalogVersionDetails Catalog { get; private set; } = default!;
    public CatalogEditorDetails Editor { get; private set; } = default!;
    public CatalogImportReport? ImportReport { get; private set; }
    public long? DeletePreviewElementId { get; private set; }
    public DeleteDocumentElementPreview? DeletePreview { get; private set; }
    public async Task<IActionResult> OnGetAsync(long id, CancellationToken ct) => await LoadAsync(id, ct);
    public async Task<IActionResult> OnPostReadyAsync(long id, long concurrencyVersion, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new SetCatalogReadyCommand(id, concurrencyVersion, true), ct));
    public async Task<IActionResult> OnPostDraftAsync(long id, long concurrencyVersion, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new SetCatalogDraftCommand(id, concurrencyVersion, true), ct));
    public async Task<IActionResult> OnPostCopyAsync(long id, long concurrencyVersion, CancellationToken ct)
    {
        var result = await mediator.Send(new CopyCatalogVersionCommand(id, concurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage(new { id = result.Value });
        result.ApplyTo(ModelState); return await LoadAsync(id, ct);
    }
    public async Task<IActionResult> OnPostAddElementAsync(long id, CancellationToken ct) => await ResultOrReload(id, await mediator.Send(new AddDocumentElementCommand(id, ElementInput.ParentElementId, ElementInput.Title, ElementInput.Text, ElementInput.Notes), ct), ct);
    public async Task<IActionResult> OnPostUpdateElementAsync(long id, CancellationToken ct) => await ResultOrReload(id, await mediator.Send(new UpdateDocumentElementCommand(ElementInput.ElementId, ElementInput.Title, ElementInput.Text, ElementInput.Notes), ct), ct);
    public async Task<IActionResult> OnPostMoveElementAsync(long id, CancellationToken ct) => await ResultOrReload(id, await mediator.Send(new MoveDocumentElementCommand(ElementInput.ElementId, ElementInput.ParentElementId, ElementInput.SortOrder), ct), ct);
    public async Task<IActionResult> OnPostPreviewDeleteElementAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDeleteDocumentElementPreviewQuery(ElementInput.ElementId), ct);
        if (result.IsSuccess) { DeletePreviewElementId = ElementInput.ElementId; DeletePreview = result.Value; }
        else result.ApplyTo(ModelState);
        return await LoadAsync(id, ct);
    }
    public async Task<IActionResult> OnPostDeleteElementAsync(long id, CancellationToken ct) => await ResultOrReload(id, await mediator.Send(new DeleteDocumentElementCommand(ElementInput.ElementId, true), ct), ct);
    public async Task<IActionResult> OnPostAddQuestionAsync(long id, CancellationToken ct) => await ResultOrReload(id, await mediator.Send(new AddQuestionCommand(QuestionInput.ElementId, QuestionInput.Text, QuestionInput.VerificationHint, QuestionInput.EvidenceHint, QuestionInput.Notes, QuestionInput.ScopeTypeIds), ct), ct);
    public async Task<IActionResult> OnPostUpdateQuestionAsync(long id, CancellationToken ct) => await ResultOrReload(id, await mediator.Send(new UpdateQuestionCommand(QuestionInput.QuestionId, QuestionInput.Text, QuestionInput.VerificationHint, QuestionInput.EvidenceHint, QuestionInput.Notes, QuestionInput.ScopeTypeIds), ct), ct);
    public async Task<IActionResult> OnPostMoveQuestionAsync(long id, CancellationToken ct) => await ResultOrReload(id, await mediator.Send(new MoveQuestionCommand(QuestionInput.QuestionId, QuestionInput.SortOrder), ct), ct);
    public async Task<IActionResult> OnPostDeleteQuestionAsync(long id, CancellationToken ct) => await ResultOrReload(id, await mediator.Send(new DeleteQuestionCommand(QuestionInput.QuestionId, true), ct), ct);
    public async Task<IActionResult> OnPostSetWeightAsync(long id, CancellationToken ct) => await ResultOrReload(id, await mediator.Send(new SetDocumentElementWeightCommand(WeightInput.ElementId, WeightInput.Weight), ct), ct);
    public async Task<IActionResult> OnPostValidateImportAsync(long id, CancellationToken ct) { var r = await mediator.Send(new ValidateCatalogImportCommand(ImportInput.PackageJson), ct); if (r.IsSuccess) ImportReport = r.Value; else r.ApplyTo(ModelState); return await LoadAsync(id, ct); }
    public async Task<IActionResult> OnPostApplyImportAsync(long id, CancellationToken ct) => await ResultOrReload(id, await mediator.Send(new ApplyCatalogImportCommand(ImportInput.PackageJson, ImportInput.Mode, ImportInput.SelectedRootElementIds), ct), ct);
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
    private async Task<IActionResult> ExecuteAsync(long id, ValueTask<Auditarium.Common.Results.Result> pending) => await ResultOrReload(id, await pending, HttpContext.RequestAborted);
    private async Task<IActionResult> ResultOrReload(long id, Auditarium.Common.Results.Result result, CancellationToken ct) { if (result.IsSuccess) return RedirectToPage(new { id }); result.ApplyTo(ModelState); return await LoadAsync(id, ct); }
    private async Task<IActionResult> ResultOrReload<T>(long id, Auditarium.Common.Results.Result<T> result, CancellationToken ct) { if (result.IsSuccess) return RedirectToPage(new { id }); result.ApplyTo(ModelState); return await LoadAsync(id, ct); }
    private async Task<IActionResult> LoadAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCatalogEditorQuery(id), ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        Editor = result.Value!; Catalog = Editor.Catalog;
        return Page();
    }
}
