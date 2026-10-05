// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Web.Components;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Audits;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public AuditState? State { get; set; }
    public IReadOnlyList<AuditListItem> Audits { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var result = await LoadAsync(0, 25, "createdAt", true, ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        Audits = result.Value!.Items; return Page();
    }

    public async Task<IActionResult> OnGetTableAsync(int page, int size, string? sort, string? direction, CancellationToken ct)
    {
        var take = size is 25 or 50 or 100 or 200 ? size : 25;
        var pageNumber = Math.Max(page, 1);
        var descending = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
        var result = await LoadAsync((pageNumber - 1) * take, take, sort ?? "createdAt", descending, ct);
        if (!result.IsSuccess) return BadRequest();

        return new JsonResult(new
        {
            last_page = Math.Max(1, (result.Value!.TotalCount + take - 1) / take),
            data = result.Value.Items.Select(audit => new
            {
                name = audit.Name,
                detailsUrl = Url.Page("Details", new { id = audit.AuditId }),
                auditUnit = audit.AuditUnitName,
                catalog = $"{audit.DocumentTitle} v{audit.CatalogVersionNumber}",
                state = StatusPresentations.Resolve(audit.AuditState).Label,
                stateTone = StatusPresentations.Resolve(audit.AuditState).Tone,
                assignment = audit.AssignedAuditorDisplayName is null ? "Nicht zugewiesen" : $"In Bearbeitung durch {audit.AssignedAuditorDisplayName}",
                createdAt = audit.CreatedAt.LocalDateTime.ToString("g")
            })
        });
    }

    private Task<Auditarium.Common.Results.Result<AuditPage>> LoadAsync(int skip, int take, string sort, bool descending, CancellationToken ct)
        => mediator.Send(new ListAuditsQuery(Search, State, skip, take, sort, descending), ct).AsTask();

}
