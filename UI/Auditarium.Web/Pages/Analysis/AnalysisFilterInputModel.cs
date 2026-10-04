// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Models.Catalog;

namespace Auditarium.Web.Pages.Analysis;

/// <summary>GET-bound presentation input for the shared audit-data selection.</summary>
public sealed class AnalysisFilterInputModel
{
    public DateTimeOffset? CreatedFrom { get; set; }
    public DateTimeOffset? CreatedTo { get; set; }
    public long? ScopeTypeId { get; set; }
    public long? AuditUnitId { get; set; }
    public long? DocumentId { get; set; }
    public long? CatalogVersionId { get; set; }
    public AuditState? AuditState { get; set; }

    public AuditDataFilter ToBllFilter() => new(CreatedFrom, CreatedTo, ScopeTypeId, AuditUnitId, DocumentId, CatalogVersionId, AuditState);
}

public enum AnalysisView { Audits, DocumentElements, Questions }
public enum TimelineGroup { ScopeType, AuditUnit, Document, CatalogVersion, AuditState }
