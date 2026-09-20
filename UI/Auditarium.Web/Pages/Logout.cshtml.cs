// SPDX-License-Identifier: MIT
using Auditarium.Infrastructure.Security;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Abstractions.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages;

public sealed class LogoutModel(IAuditEventWriter auditEvents, ICurrentActor actor, ILogger<LogoutModel> logger) : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        try { await auditEvents.WriteAsync(new AuditEvent("LOGOUT", "User", actor.UserId)); }
        catch (Exception exception) { logger.LogError(exception, "Audit log write failed while logging out."); }
        await HttpContext.SignOutAsync(AuditariumAuthenticationSchemes.Cookie);
        return RedirectToPage("Index");
    }
}
