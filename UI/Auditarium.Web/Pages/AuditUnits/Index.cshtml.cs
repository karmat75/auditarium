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
    public IReadOnlyList<AuditUnitHierarchyItem> Hierarchy { get; private set; } = [];
    public bool CanDelete { get; private set; }
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        CanDelete = await CanDeleteAsync(ct);
        var hierarchy = await mediator.Send(new GetAuditUnitHierarchyQuery(Search), ct);
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

    public async Task<IActionResult> OnGetTreeAsync(string? search, string? sort, string? direction, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAuditUnitHierarchyQuery(search, sort ?? "name", string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase)), ct);
        if (!result.IsSuccess) return BadRequest();
        var canDelete = await CanDeleteAsync(ct);

        var items = result.Value!;
        var nodes = items.ToDictionary(unit => unit.AuditUnitId, unit => new AuditUnitTreeNode(unit, Url.Page("Details", new { id = unit.AuditUnitId }), canDelete));
        var roots = new List<AuditUnitTreeNode>();
        foreach (var unit in items)
        {
            var node = nodes[unit.AuditUnitId];
            if (unit.ParentAuditUnitId is { } parentId && nodes.TryGetValue(parentId, out var parent)) parent.Children.Add(node);
            else roots.Add(node);
        }
        return new JsonResult(roots);
        /*
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
        }); */
    }

    private Task<bool> CanDeleteAsync(CancellationToken ct)
        => actor.UserId is { } userId ? permissionEvaluator.HasPermissionAsync(userId, "AuditUnits.Delete", ct) : Task.FromResult(false);

    private sealed class AuditUnitTreeNode
    {
        public AuditUnitTreeNode(AuditUnitHierarchyItem unit, string? detailsUrl, bool canDelete)
        {
            Id = unit.AuditUnitId; Name = unit.Name; ScopeTypeKey = unit.ScopeTypeKey; ScopeType = unit.ScopeTypeName;
            UsageState = StatusPresentations.Resolve(unit.UsageState).Label; UsageStateTone = StatusPresentations.Resolve(unit.UsageState).Tone;
            ConcurrencyVersion = unit.ConcurrencyVersion; DetailsUrl = detailsUrl; CanDelete = canDelete;
        }
        public long Id { get; }
        public string Name { get; }
        public string ScopeTypeKey { get; }
        public string ScopeType { get; }
        public string UsageState { get; }
        public string UsageStateTone { get; }
        public long ConcurrencyVersion { get; }
        public string? DetailsUrl { get; }
        public bool CanDelete { get; }
        public List<AuditUnitTreeNode> Children { get; } = [];
    }
}
