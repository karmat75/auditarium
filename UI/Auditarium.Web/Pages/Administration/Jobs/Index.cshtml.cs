// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Jobs;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Administration.Jobs;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    public IReadOnlyList<JobOperationsItem> Jobs { get; private set; } = [];

    public Task<IActionResult> OnGetAsync(CancellationToken ct) => LoadAsync(ct);

    public async Task<IActionResult> OnPostTriggerAsync(string jobKey, CancellationToken ct)
    {
        var result = await mediator.Send(new TriggerJobCommand(jobKey), ct);
        if (result.IsSuccess) return RedirectToPage();
        result.ApplyTo(ModelState);
        return await LoadAsync(ct);
    }

    private async Task<IActionResult> LoadAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new ListJobOperationsQuery(), ct);
        if (result.IsSuccess) Jobs = result.Value!;
        else result.ApplyTo(ModelState);
        return Page();
    }
}
