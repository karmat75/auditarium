// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Identity.AuthenticationProviders;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Administration.AuthenticationProviders;

[Authorize]
public sealed class CreateModel(IMediator mediator) : PageModel
{
    [BindProperty] public CreateProviderInputModel Input { get; set; } = new();
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();
        var result = await mediator.Send(new CreateLdapProviderCommand(Input.ProviderKey, Input.DisplayName), ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        return RedirectToPage("Details", new { id = result.Value });
    }
}
