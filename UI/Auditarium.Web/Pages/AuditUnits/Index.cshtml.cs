// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.AuditUnits;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    public IReadOnlyList<AuditUnitListItem> AuditUnits { get; private set; } = [];
    public IReadOnlyList<AuditUnitHierarchyItem> Hierarchy { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var units = await mediator.Send(new ListAuditUnitsQuery(Search, 0, 200, "name", false), ct);
        if (!units.IsSuccess) { units.ApplyTo(ModelState); return Page(); }
        AuditUnits = units.Value!.Items;
        var hierarchy = await mediator.Send(new GetAuditUnitHierarchyQuery(), ct);
        if (!hierarchy.IsSuccess) hierarchy.ApplyTo(ModelState); else Hierarchy = hierarchy.Value!;
        return Page();
    }
}
