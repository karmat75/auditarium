// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Administration.Roles;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Administration.Roles;

[Authorize]
public sealed class DetailsModel(IMediator mediator) : PageModel
{
    [BindProperty] public RoleInputModel Input { get; set; } = new();
    public RoleDetails? Role { get; private set; }
    public Task<IActionResult> OnGetAsync(long id, CancellationToken ct) => LoadAsync(id, ct);
    public async Task<IActionResult> OnPostUpdateAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateRoleCommand(id, Input.Name, Input.Description, Input.IsActive, Input.ConcurrencyVersion), ct);
        return await ResultOrRedirect(id, result, ct);
    }
    public async Task<IActionResult> OnPostAddPermissionAsync(long id, string permissionKey, CancellationToken ct) => await ResultOrRedirect(id, await mediator.Send(new AddPermissionToRoleCommand(id, permissionKey), ct), ct);
    public async Task<IActionResult> OnPostRemovePermissionAsync(long id, string permissionKey, CancellationToken ct) => await ResultOrRedirect(id, await mediator.Send(new RemovePermissionFromRoleCommand(id, permissionKey), ct), ct);
    private async Task<IActionResult> ResultOrRedirect(long id, Auditarium.Common.Results.Result result, CancellationToken ct) { if (result.IsSuccess) return RedirectToPage(new { id }); result.ApplyTo(ModelState); return await LoadAsync(id, ct, false); }
    private async Task<IActionResult> LoadAsync(long id, CancellationToken ct, bool populate = true)
    {
        var result = await mediator.Send(new GetRoleQuery(id), ct); if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        Role = result.Value; if (populate) Input = new() { Name = result.Value!.Name, Description = result.Value.Description, IsActive = result.Value.IsActive, ConcurrencyVersion = result.Value.ConcurrencyVersion }; return Page();
    }
}
