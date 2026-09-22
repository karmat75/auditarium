// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Bll.Abstractions.Identity;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Auditarium.Web.Pages;

namespace Auditarium.Web.Pages.AuditUnits;

[Authorize]
public sealed class DetailsModel(IMediator mediator, ICurrentActor actor, IPermissionEvaluator permissionEvaluator) : PageModel
{
    [BindProperty] public AuditUnitInputModel Input { get; set; } = new();
    [BindProperty] public SoftDeleteInputModel DeleteInput { get; set; } = new();
    public AuditUnitFormOptions Options { get; private set; } = new([], []);
    public bool CanDelete { get; private set; }
    public async Task<IActionResult> OnGetAsync(long id, CancellationToken ct) => await LoadAsync(id, ct);
    public async Task<IActionResult> OnPostAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateAuditUnitCommand(id, Input.ToBll(), Input.ConcurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage(new { id });
        result.ApplyTo(ModelState); return await LoadAsync(id, ct);
    }
    public async Task<IActionResult> OnPostDeleteAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAuditUnitCommand(id, DeleteInput.Reason, DeleteInput.ConcurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage("Index");
        result.ApplyTo(ModelState); return await LoadAsync(id, ct);
    }
    private async Task<IActionResult> LoadAsync(long id, CancellationToken ct)
    {
        var unit = await mediator.Send(new GetAuditUnitQuery(id), ct);
        if (!unit.IsSuccess) { unit.ApplyTo(ModelState); return Page(); }
        if (!Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)) Input = AuditUnitInputModel.From(unit.Value!);
        DeleteInput.ConcurrencyVersion = unit.Value!.ConcurrencyVersion;
        CanDelete = actor.UserId is { } userId && await permissionEvaluator.HasPermissionAsync(userId, "AuditUnits.Delete", ct);
        var options = await mediator.Send(new GetAuditUnitFormOptionsQuery(), ct);
        if (!options.IsSuccess) options.ApplyTo(ModelState); else Options = options.Value!;
        return Page();
    }
}
