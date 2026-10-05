// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Administration.Users;
using Auditarium.Web.Components;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Administration.Users;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActive { get; set; }
    public IReadOnlyList<UserListItem> Users { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var result = await LoadAsync(0, 25, "username", false, ct);
        if (!result.IsSuccess) result.ApplyTo(ModelState); else Users = result.Value!.Items;
        return Page();
    }

    public async Task<IActionResult> OnGetTableAsync(int page, int size, string? sort, string? direction, CancellationToken ct)
    {
        var take = size is 25 or 50 or 100 or 200 ? size : 25;
        var pageNumber = Math.Max(page, 1);
        var descending = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
        var result = await LoadAsync((pageNumber - 1) * take, take, sort ?? "username", descending, ct);
        if (!result.IsSuccess) return BadRequest();

        return new JsonResult(new
        {
            last_page = Math.Max(1, (result.Value!.TotalCount + take - 1) / take),
            data = result.Value.Items.Select(user => new
            {
                username = user.Username,
                detailsUrl = Url.Page("Details", new { id = user.UserId }),
                displayName = user.DisplayName,
                email = user.Email ?? "–",
                status = StatusPresentations.Resolve(user.IsActive ? "ACTIVE" : "INACTIVE").Label,
                statusTone = StatusPresentations.Resolve(user.IsActive ? "ACTIVE" : "INACTIVE").Tone
            })
        });
    }

    private Task<Auditarium.Common.Results.Result<UserPage>> LoadAsync(int skip, int take, string sort, bool descending, CancellationToken ct)
        => mediator.Send(new ListUsersQuery(Search, IsActive, skip, take, sort, descending), ct).AsTask();
}
