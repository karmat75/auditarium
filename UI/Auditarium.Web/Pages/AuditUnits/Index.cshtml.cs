// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Features.Audits;
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
    public bool CanManage { get; private set; }
    public bool CanDelete { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await LoadPermissionsAsync(ct);
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
        await LoadPermissionsAsync(ct);
        var nodes = result.Value!.ToDictionary(unit => unit.AuditUnitId, unit => new AuditUnitTreeNode(unit, Url.Page("Details", new { id = unit.AuditUnitId }), Url.Page("Edit", new { id = unit.AuditUnitId }), CanManage, CanDelete));
        var roots = new List<AuditUnitTreeNode>();
        foreach (var unit in result.Value!)
        {
            var node = nodes[unit.AuditUnitId];
            if (unit.ParentAuditUnitId is { } parentId && nodes.TryGetValue(parentId, out var parent)) parent.Children.Add(node);
            else roots.Add(node);
        }
        return new JsonResult(roots);
    }

    private async Task LoadPermissionsAsync(CancellationToken ct)
    {
        if (actor.UserId is not { } userId) return;
        var permissions = await permissionEvaluator.GetPermissionsAsync(userId, ct);
        CanManage = permissions.Contains("AuditUnits.Manage");
        CanDelete = permissions.Contains("AuditUnits.Delete");
    }

    private sealed class AuditUnitTreeNode
    {
        public AuditUnitTreeNode(AuditUnitHierarchyItem unit, string? detailsUrl, string? editUrl, bool canManage, bool canDelete)
        {
            Id = unit.AuditUnitId; Name = unit.Name; ScopeTypeKey = unit.ScopeTypeKey; ScopeType = unit.ScopeTypeName; UsageState = StatusPresentations.Resolve(unit.UsageState).Label; UsageStateTone = StatusPresentations.Resolve(unit.UsageState).Tone; ConcurrencyVersion = unit.ConcurrencyVersion; DetailsUrl = detailsUrl; EditUrl = editUrl; CanManage = canManage; CanDelete = canDelete;
        }
        public long Id { get; }
        public string Name { get; }
        public string ScopeTypeKey { get; }
        public string ScopeType { get; }
        public string UsageState { get; }
        public string UsageStateTone { get; }
        public long ConcurrencyVersion { get; }
        public string? DetailsUrl { get; }
        public string? EditUrl { get; }
        public bool CanManage { get; }
        public bool CanDelete { get; }
        public List<AuditUnitTreeNode> Children { get; } = [];
    }
}
