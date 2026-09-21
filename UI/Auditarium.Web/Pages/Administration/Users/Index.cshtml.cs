// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Administration.Users;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Administration.Users;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActive { get; set; }
    public IReadOnlyList<UserListItem> Users { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new ListUsersQuery(Search, IsActive, 0, 200, "username", false), ct);
        if (!result.IsSuccess) result.ApplyTo(ModelState); else Users = result.Value!.Items;
        return Page();
    }
}
