// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Catalog;
using Auditarium.Web.Components;
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
        var result = await LoadAsync(0, 25, "title", false, ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        Documents = result.Value!.Items;
        return Page();
    }

    public async Task<IActionResult> OnGetTableAsync(int page, int size, string? sort, string? direction, CancellationToken ct)
    {
        var take = size is 25 or 50 or 100 or 200 ? size : 25;
        var pageNumber = Math.Max(page, 1);
        var descending = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
        var result = await LoadAsync((pageNumber - 1) * take, take, sort ?? "title", descending, ct);
        if (!result.IsSuccess) return BadRequest();

        return new JsonResult(new
        {
            last_page = Math.Max(1, (result.Value!.TotalCount + take - 1) / take),
            data = result.Value.Items.Select(document => new
            {
                title = document.Title,
                detailsUrl = Url.Page("Details", new { id = document.DocumentId }),
                publisher = document.Publisher ?? "–",
                version = document.Version ?? "–",
                usageState = StatusPresentations.Resolve(document.UsageState).Label,
                usageStateTone = StatusPresentations.Resolve(document.UsageState).Tone
            })
        });
    }

    private Task<Auditarium.Common.Results.Result<DocumentPage>> LoadAsync(int skip, int take, string sort, bool descending, CancellationToken ct)
        => mediator.Send(new ListDocumentsQuery(Search, skip, take, sort, descending), ct).AsTask();
}
