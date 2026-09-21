// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Audits;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public AuditState? State { get; set; }
    public IReadOnlyList<AuditListItem> Audits { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new ListAuditsQuery(Search, State, 0, 200, "createdAt", true), ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        Audits = result.Value!.Items; return Page();
    }
}
