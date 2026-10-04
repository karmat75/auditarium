// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Analysis;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)] public AnalysisFilterInputModel Filter { get; set; } = new();
    [BindProperty(SupportsGet = true)] public AnalysisView View { get; set; } = AnalysisView.Audits;
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public bool Descending { get; set; } = true;

    public AuditDataPage<AuditDataAuditRow>? Audits { get; private set; }
    public AuditDataPage<AuditDataElementRow>? Elements { get; private set; }
    public AuditDataPage<AuditDataQuestionRow>? Questions { get; private set; }
    public int PageSize => 50;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (PageNumber < 1) PageNumber = 1;
        var skip = (PageNumber - 1) * PageSize;
        var filter = Filter.ToBllFilter();
        switch (View)
        {
            case AnalysisView.Audits:
                {
                    var audits = await mediator.Send(new ListAuditDataAuditsQuery(filter, skip, PageSize, AuditDataAuditSort.CreatedAt, Descending), ct);
                    if (!audits.IsSuccess) { audits.ApplyTo(ModelState); return Page(); }
                    Audits = audits.Value;
                    break;
                }
            case AnalysisView.DocumentElements:
                {
                    var elements = await mediator.Send(new ListAuditDataElementsQuery(filter, skip, PageSize, AuditDataElementSort.Weight, Descending), ct);
                    if (!elements.IsSuccess) { elements.ApplyTo(ModelState); return Page(); }
                    Elements = elements.Value;
                    break;
                }
            case AnalysisView.Questions:
                {
                    var questions = await mediator.Send(new ListAuditDataQuestionsQuery(filter, skip, PageSize, AuditDataQuestionSort.CreatedAt, Descending), ct);
                    if (!questions.IsSuccess) { questions.ApplyTo(ModelState); return Page(); }
                    Questions = questions.Value;
                    break;
                }
        }

        return Page();
    }
}
