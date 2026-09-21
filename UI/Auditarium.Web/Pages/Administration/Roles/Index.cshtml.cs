// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Administration.Roles;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Administration.Roles;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    public IReadOnlyList<RoleListItem> Roles { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken ct)
    { var result = await mediator.Send(new ListRolesQuery(), ct); if (result.IsSuccess) Roles = result.Value!; else result.ApplyTo(ModelState); }
}
