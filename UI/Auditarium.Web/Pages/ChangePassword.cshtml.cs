// SPDX-License-Identifier: MIT
using System.Security.Claims;
using Auditarium.Bll.Features.Identity.LocalCredentials;
using Auditarium.Infrastructure.Security;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages;

public sealed class ChangePasswordModel(IMediator mediator) : PageModel
{
    [BindProperty] public ChangePasswordInputModel Input { get; set; } = new();
    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? Page() : RedirectToPage("Login");
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return RedirectToPage("Login");
        }
        var result = await mediator.Send(new ChangeCurrentPasswordCommand(Input.CurrentPassword, Input.NewPassword), cancellationToken);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString(System.Globalization.CultureInfo.InvariantCulture)) };
        await HttpContext.SignInAsync(AuditariumAuthenticationSchemes.Cookie, new ClaimsPrincipal(new ClaimsIdentity(claims, AuditariumAuthenticationSchemes.Cookie)));
        return RedirectToPage("Index");
    }
}

public sealed class ChangePasswordInputModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
