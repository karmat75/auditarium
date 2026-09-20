// SPDX-License-Identifier: MIT
using System.Security.Claims;
using Auditarium.Bll.Features.Identity.LocalCredentials;
using Auditarium.Infrastructure.Security;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages;

public sealed class LoginModel(IMediator mediator) : PageModel
{
    [BindProperty] public LoginInputModel Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AuthenticateInteractiveUserCommand(Input.Username, Input.Password, Input.Provider), cancellationToken);
        if (!result.IsSuccess)
        {
            result.ApplyTo(ModelState);
            return Page();
        }
        var authentication = result.Value!;
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, authentication.UserId.ToString(System.Globalization.CultureInfo.InvariantCulture)) };
        if (authentication.MustChangePassword) claims.Add(new Claim(AuditariumAuthenticationSchemes.MustChangePasswordClaim, "true"));
        await HttpContext.SignInAsync(AuditariumAuthenticationSchemes.Cookie, new ClaimsPrincipal(new ClaimsIdentity(claims, AuditariumAuthenticationSchemes.Cookie)));
        if (authentication.MustChangePassword) return RedirectToPage("ChangePassword");
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/");
    }
}

public sealed class LoginInputModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Provider { get; set; }
}
