// SPDX-License-Identifier: MIT
using System.Security.Claims;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages;

public sealed class LoginModel(IAuthenticationRouter router, ILocalAuthenticationService localAuthentication, ILdapAuthenticationService ldapAuthentication) : PageModel
{
    [BindProperty] public string Username { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public string? Provider { get; set; }
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)) { ModelState.AddModelError(string.Empty, "Benutzername und Passwort sind erforderlich."); return Page(); }
        var provider = await router.RouteAsync(Username, Provider, cancellationToken);
        AuthenticationSuccess? result = provider switch
        {
            "LOCAL" => await localAuthentication.AuthenticateAsync(new AuthenticationAttempt(Username, Password, provider), cancellationToken),
            not null => await ldapAuthentication.AuthenticateAsync(provider, new AuthenticationAttempt(Username, Password, provider), cancellationToken),
            _ => null
        };
        if (result is null) { ModelState.AddModelError(string.Empty, "Anmeldung nicht möglich."); return Page(); }
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, result.UserId.ToString(System.Globalization.CultureInfo.InvariantCulture)) };
        if (result.MustChangePassword) claims.Add(new Claim(AuditariumAuthenticationSchemes.MustChangePasswordClaim, "true"));
        await HttpContext.SignInAsync(AuditariumAuthenticationSchemes.Cookie, new ClaimsPrincipal(new ClaimsIdentity(claims, AuditariumAuthenticationSchemes.Cookie)));
        if (result.MustChangePassword) return RedirectToPage("ChangePassword");
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/");
    }
}
