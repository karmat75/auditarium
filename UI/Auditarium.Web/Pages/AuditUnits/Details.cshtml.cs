// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Features.Audits;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.AuditUnits;

[Authorize]
public sealed class DetailsModel(IMediator mediator, ICurrentActor actor, IPermissionEvaluator permissionEvaluator) : PageModel
{
    public AuditUnitDetails Unit { get; private set; } = new(0, null, 0, string.Empty, null, AuditUnitUsageState.Active, null, null, 0);
    public AuditUnitFormOptions Options { get; private set; } = new([], []);
    public string? ParentName => Options.AuditUnits.SingleOrDefault(unit => unit.AuditUnitId == Unit.ParentAuditUnitId)?.Name;
    public string? ScopeTypeName => Options.ScopeTypes.SingleOrDefault(scope => scope.ScopeTypeId == Unit.ScopeTypeId)?.Name;
    public bool CanManage { get; private set; }
    public async Task<IActionResult> OnGetAsync(long id, CancellationToken ct)
    {
        CanManage = actor.UserId is { } userId && await permissionEvaluator.HasPermissionAsync(userId, "AuditUnits.Manage", ct);
        var unit = await mediator.Send(new GetAuditUnitQuery(id), ct);
        if (!unit.IsSuccess) { unit.ApplyTo(ModelState); return Page(); }
        Unit = unit.Value!;
        var options = await mediator.Send(new GetAuditUnitFormOptionsQuery(), ct);
        if (!options.IsSuccess) options.ApplyTo(ModelState); else Options = options.Value!;
        return Page();
    }
}
