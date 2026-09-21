// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Administration.Roles;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Administration.Roles;

[Authorize]
public sealed class CreateModel(IMediator mediator) : PageModel
{
    [BindProperty] public RoleInputModel Input { get; set; } = new();
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();
        var result = await mediator.Send(new CreateRoleCommand(Input.Name, Input.Description, Input.IsActive), ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        return RedirectToPage("Details", new { id = result.Value });
    }
}
