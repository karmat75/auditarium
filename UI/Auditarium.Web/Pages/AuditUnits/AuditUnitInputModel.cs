// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Models.Catalog;

namespace Auditarium.Web.Pages.AuditUnits;

public sealed class AuditUnitInputModel
{
    public long AuditUnitId { get; set; }
    public long? ParentAuditUnitId { get; set; }
    public long ScopeTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AuditUnitUsageState UsageState { get; set; } = AuditUnitUsageState.Active;
    public string? UsageStateReason { get; set; }
    public string? Notes { get; set; }
    public long ConcurrencyVersion { get; set; }
    public AuditUnitInput ToBll() => new(ParentAuditUnitId, ScopeTypeId, Name, Description, UsageState, UsageStateReason, Notes);
    public static AuditUnitInputModel From(AuditUnitDetails item) => new() { AuditUnitId = item.AuditUnitId, ParentAuditUnitId = item.ParentAuditUnitId, ScopeTypeId = item.ScopeTypeId, Name = item.Name, Description = item.Description, UsageState = item.UsageState, UsageStateReason = item.UsageStateReason, Notes = item.Notes, ConcurrencyVersion = item.ConcurrencyVersion };
}
