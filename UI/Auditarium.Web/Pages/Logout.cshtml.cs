// SPDX-License-Identifier: MIT
using Auditarium.Infrastructure.Security;
using Auditarium.Bll.Features.Identity.LocalCredentials;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages;

public sealed class LogoutModel(IMediator mediator, ILogger<LogoutModel> logger) : PageModel
{
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try { await mediator.Send(new RecordLogoutCommand(), cancellationToken); }
        catch (Exception exception) { logger.LogError(exception, "Audit log write failed while logging out."); }
        await HttpContext.SignOutAsync(AuditariumAuthenticationSchemes.Cookie);
        return RedirectToPage("Index");
    }
}
