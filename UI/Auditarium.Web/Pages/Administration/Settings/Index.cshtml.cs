// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Administration.Settings;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Administration.Settings;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    [BindProperty] public SettingInputModel Input { get; set; } = new();
    public IReadOnlyList<SettingDetails> Settings { get; private set; } = [];
    public Task<IActionResult> OnGetAsync(CancellationToken ct) => LoadAsync(ct);
    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateSettingCommand(Input.Key, Input.Value, Input.ConcurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage(); result.ApplyTo(ModelState); return await LoadAsync(ct);
    }
    private async Task<IActionResult> LoadAsync(CancellationToken ct)
    { var result = await mediator.Send(new ListSettingsQuery(), ct); if (result.IsSuccess) Settings = result.Value!; else result.ApplyTo(ModelState); return Page(); }
}
