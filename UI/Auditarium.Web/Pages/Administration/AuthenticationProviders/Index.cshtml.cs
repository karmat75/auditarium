// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Identity.AuthenticationProviders;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Administration.AuthenticationProviders;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    public IReadOnlyList<AuthenticationProviderListItem> Providers { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken ct) { var result = await mediator.Send(new ListAuthenticationProvidersQuery(), ct); if (result.IsSuccess) Providers = result.Value!; else result.ApplyTo(ModelState); }
}
