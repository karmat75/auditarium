// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auditarium.Web.Pages.Exports;

[Authorize]
public sealed class IndexModel(IMediator mediator) : PageModel
{
    [BindProperty] public ExportAuditDataInputModel Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();
        var result = await mediator.Send(new ExportAuditDataCsvQuery(Input.Level, Input.ToBllFilter()), ct);
        if (!result.IsSuccess)
        {
            result.ApplyTo(ModelState);
            return Page();
        }

        return File(result.Value!.Content, "text/csv; charset=utf-8", result.Value.FileName);
    }
}

public sealed class ExportAuditDataInputModel
{
    public AuditCsvExportLevel Level { get; set; } = AuditCsvExportLevel.Audits;
    public DateTimeOffset? CreatedFrom { get; set; }
    public DateTimeOffset? CreatedTo { get; set; }
    public long? ScopeTypeId { get; set; }
    public long? AuditUnitId { get; set; }
    public long? DocumentId { get; set; }
    public long? CatalogVersionId { get; set; }
    public Auditarium.Models.Catalog.AuditState? AuditState { get; set; }

    public AuditDataFilter ToBllFilter() => new(CreatedFrom, CreatedTo, ScopeTypeId, AuditUnitId, DocumentId, CatalogVersionId, AuditState);
}
