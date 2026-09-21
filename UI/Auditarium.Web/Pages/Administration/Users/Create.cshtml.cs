// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Administration.Users;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Administration.Users;

[Authorize]
public sealed class CreateModel(IMediator mediator) : PageModel
{
    [BindProperty] public UserInputModel Input { get; set; } = new();
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();
        var result = await mediator.Send(new CreateUserCommand(Input.Username, Input.DisplayName, Input.Email, Input.IsActive), ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        return RedirectToPage("Details", new { id = result.Value });
    }
}
