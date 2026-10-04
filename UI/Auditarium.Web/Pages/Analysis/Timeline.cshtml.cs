// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Analysis;

[Authorize]
public sealed class TimelineModel(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)] public AnalysisFilterInputModel Filter { get; set; } = new();
    [BindProperty(SupportsGet = true)] public TimelineGroup GroupBy { get; set; } = TimelineGroup.AuditUnit;
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public AuditDataPage<AuditDataAuditRow>? Audits { get; private set; }
    public int PageSize => 50;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (PageNumber < 1) PageNumber = 1;
        var result = await mediator.Send(new ListAuditDataAuditsQuery(Filter.ToBllFilter(), (PageNumber - 1) * PageSize, PageSize, AuditDataAuditSort.CreatedAt, false), ct);
        if (!result.IsSuccess) { result.ApplyTo(ModelState); return Page(); }
        Audits = result.Value;
        return Page();
    }

    public string GroupLabel(AuditDataAuditRow row) => GroupBy switch
    {
        TimelineGroup.ScopeType => row.Audit.ScopeTypeName,
        TimelineGroup.AuditUnit => row.Audit.AuditUnitName,
        TimelineGroup.Document => row.Audit.DocumentTitle,
        TimelineGroup.CatalogVersion => $"{row.Audit.DocumentTitle} · Katalogversion {row.Audit.CatalogVersionNumber}",
        TimelineGroup.AuditState => row.Audit.AuditState.ToString(),
        _ => throw new InvalidOperationException("Unsupported timeline grouping.")
    };
}
