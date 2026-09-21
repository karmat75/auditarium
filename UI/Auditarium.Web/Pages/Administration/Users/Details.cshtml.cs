// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Administration.Users;
using Auditarium.Bll.Features.Identity.ApiCredentials;
using Auditarium.Bll.Abstractions.Identity;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Auditarium.Web.Pages.Administration.Users;

[Authorize]
public sealed class DetailsModel(IMediator mediator, IPermissionEvaluator permissionEvaluator) : PageModel
{
    [BindProperty] public UserInputModel Input { get; set; } = new();
    [BindProperty] public IdentityInputModel IdentityInput { get; set; } = new();
    [BindProperty] public CredentialInputModel CredentialInput { get; set; } = new();
    public UserDetails? UserDetails { get; private set; }
    public IReadOnlyList<IdentityProviderOption> Providers { get; private set; } = [];
    public IReadOnlyList<ApiCredentialMetadata> Credentials { get; private set; } = [];
    public bool CanManageCredentials { get; private set; }
    [TempData] public string? CreatedCredential { get; set; }

    public Task<IActionResult> OnGetAsync(long id, CancellationToken ct) => LoadAsync(id, ct);
    public async Task<IActionResult> OnPostUpdateAsync(long id, CancellationToken ct)
    {
        if (!ModelState.IsValid) return await LoadAsync(id, ct, false);
        var result = await mediator.Send(new UpdateUserCommand(id, Input.Username, Input.DisplayName, Input.Email, Input.IsActive, Input.ConcurrencyVersion), ct);
        return await ResultOrRedirect(id, result, ct);
    }
    public async Task<IActionResult> OnPostAddRoleAsync(long id, long roleId, CancellationToken ct) => await ResultOrRedirect(id, await mediator.Send(new AddUserRoleCommand(id, roleId), ct), ct);
    public async Task<IActionResult> OnPostRemoveRoleAsync(long id, long roleId, CancellationToken ct) => await ResultOrRedirect(id, await mediator.Send(new RemoveUserRoleCommand(id, roleId), ct), ct);
    public async Task<IActionResult> OnPostAddIdentityAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new AddUserIdentityCommand(id, IdentityInput.ProviderId, IdentityInput.ExternalId), ct);
        if (result.IsSuccess) return RedirectToPage(new { id });
        result.ApplyTo(ModelState); return await LoadAsync(id, ct, false);
    }
    public async Task<IActionResult> OnPostCreateCredentialAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateApiCredentialCommand(CredentialInput.IdentityId, CredentialInput.Name, CredentialInput.ExpiresAt), ct);
        if (result.IsSuccess) { CreatedCredential = result.Value!.Credential; return RedirectToPage(new { id }); }
        result.ApplyTo(ModelState); return await LoadAsync(id, ct, false);
    }
    public async Task<IActionResult> OnPostRotateCredentialAsync(long id, long credentialId, CancellationToken ct)
    {
        var result = await mediator.Send(new RotateApiCredentialCommand(credentialId, CredentialInput.Name, CredentialInput.ExpiresAt), ct);
        if (result.IsSuccess) { CreatedCredential = result.Value!.Credential; return RedirectToPage(new { id }); }
        result.ApplyTo(ModelState); return await LoadAsync(id, ct, false);
    }
    public async Task<IActionResult> OnPostRevokeCredentialAsync(long id, long credentialId, CancellationToken ct) => await ResultOrRedirect(id, await mediator.Send(new RevokeApiCredentialCommand(credentialId), ct), ct);

    private async Task<IActionResult> ResultOrRedirect(long id, Auditarium.Common.Results.Result result, CancellationToken ct)
    { if (result.IsSuccess) return RedirectToPage(new { id }); result.ApplyTo(ModelState); return await LoadAsync(id, ct, false); }
    private async Task<IActionResult> LoadAsync(long id, CancellationToken ct, bool populateInput = true)
    {
        var user = await mediator.Send(new GetUserQuery(id), ct); if (!user.IsSuccess) { user.ApplyTo(ModelState); return Page(); }
        UserDetails = user.Value;
        if (populateInput) Input = new() { Username = user.Value!.Username, DisplayName = user.Value.DisplayName, Email = user.Value.Email, IsActive = user.Value.IsActive, ConcurrencyVersion = user.Value.ConcurrencyVersion };
        var providers = await mediator.Send(new ListIdentityProviderOptionsQuery(), ct); if (providers.IsSuccess) Providers = providers.Value!; else providers.ApplyTo(ModelState);
        if (long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId))
            CanManageCredentials = await permissionEvaluator.HasPermissionAsync(currentUserId, "Authentication.Manage", ct);
        if (CanManageCredentials)
        {
            var credentials = await mediator.Send(new ListApiCredentialsQuery(id), ct); if (credentials.IsSuccess) Credentials = credentials.Value!; else credentials.ApplyTo(ModelState);
        }
        return Page();
    }
}
