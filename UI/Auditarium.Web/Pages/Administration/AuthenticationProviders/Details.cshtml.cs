// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Identity.AuthenticationProviders;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Administration.AuthenticationProviders;

[Authorize]
public sealed class DetailsModel(IMediator mediator) : PageModel
{
    [BindProperty] public ProviderMetadataInputModel MetadataInput { get; set; } = new();
    [BindProperty] public ProviderSettingInputModel SettingInput { get; set; } = new();
    [BindProperty] public ProviderStateInputModel StateInput { get; set; } = new();
    [BindProperty] public ProviderDeleteInputModel DeleteInput { get; set; } = new();
    public AuthenticationProviderDetails? Provider { get; private set; }
    [TempData] public string? StatusMessage { get; set; }

    public Task<IActionResult> OnGetAsync(long id, CancellationToken ct) => LoadAsync(id, ct);
    public async Task<IActionResult> OnPostUpdateMetadataAsync(long id, CancellationToken ct) => await ResultOrRedirect(id, await mediator.Send(new UpdateLdapProviderCommand(id, MetadataInput.DisplayName, new Dictionary<string, string?>(), new Dictionary<string, long>(), MetadataInput.ConcurrencyVersion), ct), ct);
    public async Task<IActionResult> OnPostUpdateSettingAsync(long id, CancellationToken ct)
    {
        var values = new Dictionary<string, string?> { [SettingInput.Property] = SettingInput.Value };
        var versions = new Dictionary<string, long> { [SettingInput.Property] = SettingInput.ConcurrencyVersion };
        var result = await mediator.Send(new GetAuthenticationProviderQuery(id), ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return await LoadAsync(id, ct, false); }
        return await ResultOrRedirect(id, await mediator.Send(new UpdateLdapProviderCommand(id, result.Value!.DisplayName, values, versions, SettingInput.ProviderConcurrencyVersion), ct), ct);
    }
    public async Task<IActionResult> OnPostResetSettingAsync(long id, CancellationToken ct) => await ResultOrRedirect(id, await mediator.Send(new ResetLdapProviderSettingCommand(id, SettingInput.Property, SettingInput.ConcurrencyVersion), ct), ct);
    public async Task<IActionResult> OnPostSetEnabledAsync(long id, CancellationToken ct) => await ResultOrRedirect(id, await mediator.Send(new SetAuthenticationProviderEnabledCommand(id, StateInput.IsEnabled, StateInput.ConcurrencyVersion), ct), ct);
    public async Task<IActionResult> OnPostTestAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new TestLdapProviderConnectionCommand(id), ct);
        if (result.IsSuccess) { StatusMessage = "LDAP-Verbindung erfolgreich getestet."; return RedirectToPage(new { id }); }
        result.ApplyTo(ModelState); return await LoadAsync(id, ct, false);
    }
    public async Task<IActionResult> OnPostDeleteAsync(long id, CancellationToken ct)
    {
        if (!DeleteInput.Confirmed) { ModelState.AddModelError(string.Empty, "Bestätigen Sie das Löschen."); return await LoadAsync(id, ct, false); }
        var result = await mediator.Send(new DeleteAuthenticationProviderCommand(id, DeleteInput.ConcurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage("Index"); result.ApplyTo(ModelState); return await LoadAsync(id, ct, false);
    }
    private async Task<IActionResult> ResultOrRedirect(long id, Auditarium.Common.Results.Result result, CancellationToken ct) { if (result.IsSuccess) return RedirectToPage(new { id }); result.ApplyTo(ModelState); return await LoadAsync(id, ct, false); }
    private async Task<IActionResult> LoadAsync(long id, CancellationToken ct, bool populate = true)
    {
        var result = await mediator.Send(new GetAuthenticationProviderQuery(id), ct); if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        Provider = result.Value; if (populate) MetadataInput = new() { DisplayName = result.Value!.DisplayName, ConcurrencyVersion = result.Value.ConcurrencyVersion }; return Page();
    }
}
