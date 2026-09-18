// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.System.GetHostStatus;
using Mediator;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages;

public sealed class IndexModel(IMediator mediator) : PageModel
{
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        _ = await mediator.Send(new GetHostStatusQuery(), cancellationToken);
    }
}
