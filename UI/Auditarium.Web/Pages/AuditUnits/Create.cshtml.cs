// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Bll.Features.Catalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.AuditUnits;

[Authorize]
public sealed class CreateModel(IMediator mediator) : PageModel
{
    [BindProperty] public AuditUnitInputModel Input { get; set; } = new();
    public AuditUnitFormOptions Options { get; private set; } = new([], []);
    public async Task<IActionResult> OnGetAsync(CancellationToken ct) => await LoadAsync(ct);
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new CreateAuditUnitCommand(Input.ToBll()), ct);
        if (result.IsSuccess) return RedirectToPage("Details", new { id = result.Value });
        result.ApplyTo(ModelState); return await LoadAsync(ct);
    }
    private async Task<IActionResult> LoadAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAuditUnitFormOptionsQuery(), ct);
        if (!result.IsSuccess) result.ApplyTo(ModelState); else Options = result.Value!;
        return Page();
    }
}
