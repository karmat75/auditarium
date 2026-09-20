// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Catalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Documents;

[Authorize]
public sealed class DetailsModel(IMediator mediator) : PageModel
{
    [BindProperty] public DocumentInputModel Input { get; set; } = new();
    [BindProperty] public string? NewCatalogNotes { get; set; }
    public IReadOnlyList<CatalogVersionListItem> Catalogs { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync(long id, CancellationToken ct) => await LoadAsync(id, ct);
    public async Task<IActionResult> OnPostAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateDocumentCommand(id, Input.ToBll(), Input.ConcurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage(new { id });
        result.ApplyTo(ModelState); return await LoadAsync(id, ct);
    }
    public async Task<IActionResult> OnPostCreateCatalogAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateCatalogVersionCommand(id, NewCatalogNotes), ct);
        if (result.IsSuccess) return RedirectToPage("Catalog", new { id = result.Value });
        result.ApplyTo(ModelState); return await LoadAsync(id, ct);
    }
    private async Task<IActionResult> LoadAsync(long id, CancellationToken ct)
    {
        var document = await mediator.Send(new GetDocumentQuery(id), ct);
        if (!document.IsSuccess) { document.ApplyTo(ModelState); return Page(); }
        Input = DocumentInputModel.From(document.Value!);
        var catalogs = await mediator.Send(new ListCatalogVersionsQuery(id), ct);
        if (!catalogs.IsSuccess) catalogs.ApplyTo(ModelState); else Catalogs = catalogs.Value!;
        return Page();
    }
}
