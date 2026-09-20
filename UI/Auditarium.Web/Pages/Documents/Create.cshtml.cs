// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Catalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Documents;

[Authorize]
public sealed class CreateModel(IMediator mediator) : PageModel
{
    [BindProperty] public DocumentInputModel Input { get; set; } = new();
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new CreateDocumentCommand(Input.ToBll()), ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        return RedirectToPage("Details", new { id = result.Value });
    }
}
