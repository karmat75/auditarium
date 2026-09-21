// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Audits;

[Authorize]
public sealed class DetailsModel(IMediator mediator) : PageModel
{
    [BindProperty] public AuditInputModel Input { get; set; } = new();
    [BindProperty] public PublishAuditInputModel PublishInput { get; set; } = new();
    public AuditDetails Audit { get; private set; } = default!;
    public AuditConfigurationOptions Options { get; private set; } = new([], [], []);
    public AuditPreview? Preview { get; private set; }
    public async Task<IActionResult> OnGetAsync(long id, CancellationToken ct) => await LoadAsync(id, ct);
    public async Task<IActionResult> OnPostAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateAuditDraftCommand(id, Input.ToBll(), Input.ConcurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage(new { id });
        result.ApplyTo(ModelState); return await LoadAsync(id, ct);
    }
    public async Task<IActionResult> OnPostPublishAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new PublishAuditCommand(id, PublishInput.ConcurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage(new { id });
        result.ApplyTo(ModelState); return await LoadAsync(id, ct, reloadInput: true);
    }
    private async Task<IActionResult> LoadAsync(long id, CancellationToken ct, bool reloadInput = false)
    {
        var audit = await mediator.Send(new GetAuditQuery(id), ct);
        if (!audit.IsSuccess) { audit.ApplyTo(ModelState); return Page(); }
        Audit = audit.Value!;
        if (reloadInput || !Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)) Input = AuditInputModel.From(Audit);
        PublishInput.ConcurrencyVersion = Audit.ConcurrencyVersion;
        if (Audit.AuditState == AuditState.Draft)
        {
            var options = await mediator.Send(new GetAuditConfigurationOptionsQuery(), ct);
            if (!options.IsSuccess) options.ApplyTo(ModelState); else Options = options.Value!;
            var preview = await mediator.Send(new GetAuditPreviewQuery(id), ct);
            if (!preview.IsSuccess) preview.ApplyTo(ModelState); else Preview = preview.Value;
        }
        return Page();
    }
}
