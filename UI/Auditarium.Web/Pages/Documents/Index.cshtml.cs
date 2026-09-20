// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Catalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Documents;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    public IReadOnlyList<DocumentListItem> Documents { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new ListDocumentsQuery(Search, 0, 200, "title", false), ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        Documents = result.Value!.Items;
        return Page();
    }
}
