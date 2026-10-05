// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Web.Components;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.AuditUnits;

[Authorize]
public sealed class IndexModel(IMediator mediator, ICurrentActor actor, IPermissionEvaluator permissionEvaluator) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    public IReadOnlyList<AuditUnitListItem> AuditUnits { get; private set; } = [];
    public IReadOnlyList<AuditUnitHierarchyItem> Hierarchy { get; private set; } = [];
    public bool CanDelete { get; private set; }
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var units = await LoadAsync(0, 25, "name", false, ct);
        if (!units.IsSuccess) { units.ApplyTo(ModelState); return Page(); }
        AuditUnits = units.Value!.Items;
        CanDelete = await CanDeleteAsync(ct);
        var hierarchy = await mediator.Send(new GetAuditUnitHierarchyQuery(), ct);
        if (!hierarchy.IsSuccess) hierarchy.ApplyTo(ModelState); else Hierarchy = hierarchy.Value!;
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id, long concurrencyVersion, string? reason, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAuditUnitCommand(id, reason, concurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage(new { Search });
        result.ApplyTo(ModelState);
        return await OnGetAsync(ct);
    }

    public async Task<IActionResult> OnGetTableAsync(int page, int size, string? sort, string? direction, CancellationToken ct)
    {
        var take = size is 25 or 50 or 100 or 200 ? size : 25;
        var pageNumber = Math.Max(page, 1);
        var descending = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
        var result = await LoadAsync((pageNumber - 1) * take, take, sort ?? "name", descending, ct);
        if (!result.IsSuccess) return BadRequest();
        var canDelete = await CanDeleteAsync(ct);

        return new JsonResult(new
        {
            last_page = Math.Max(1, (result.Value!.TotalCount + take - 1) / take),
            data = result.Value.Items.Select(unit => new
            {
                name = unit.Name,
                detailsUrl = Url.Page("Details", new { id = unit.AuditUnitId }),
                parent = unit.ParentName ?? "–",
                scopeType = unit.ScopeTypeName,
                usageState = StatusPresentations.Resolve(unit.UsageState).Label,
                usageStateTone = StatusPresentations.Resolve(unit.UsageState).Tone,
                id = unit.AuditUnitId,
                concurrencyVersion = unit.ConcurrencyVersion,
                canDelete
            })
        });
    }

    private Task<Auditarium.Common.Results.Result<AuditUnitPage>> LoadAsync(int skip, int take, string sort, bool descending, CancellationToken ct)
        => mediator.Send(new ListAuditUnitsQuery(Search, skip, take, sort, descending), ct).AsTask();

    private Task<bool> CanDeleteAsync(CancellationToken ct)
        => actor.UserId is { } userId ? permissionEvaluator.HasPermissionAsync(userId, "AuditUnits.Delete", ct) : Task.FromResult(false);
}
