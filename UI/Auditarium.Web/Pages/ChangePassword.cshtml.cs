// SPDX-License-Identifier: MIT
using System.Security.Claims;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages;

public sealed class ChangePasswordModel(ILocalAuthenticationService localAuthentication) : PageModel
{
    [BindProperty] public string CurrentPassword { get; set; } = string.Empty;
    [BindProperty] public string NewPassword { get; set; } = string.Empty;
    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? Page() : RedirectToPage("Login");
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) || !await localAuthentication.ChangePasswordAsync(userId, CurrentPassword, NewPassword, cancellationToken))
        {
            ModelState.AddModelError(string.Empty, "Passwortwechsel nicht möglich."); return Page();
        }
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString(System.Globalization.CultureInfo.InvariantCulture)) };
        await HttpContext.SignInAsync(AuditariumAuthenticationSchemes.Cookie, new ClaimsPrincipal(new ClaimsIdentity(claims, AuditariumAuthenticationSchemes.Cookie)));
        return RedirectToPage("Index");
    }
}
