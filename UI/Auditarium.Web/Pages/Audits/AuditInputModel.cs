// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;

namespace Auditarium.Web.Pages.Audits;

public sealed class AuditInputModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long AuditUnitId { get; set; }
    public long CatalogVersionId { get; set; }
    public bool YesCommentRequired { get; set; }
    public bool YesEvidenceRequired { get; set; }
    public bool NoCommentRequired { get; set; }
    public bool NoEvidenceRequired { get; set; }
    public bool NotApplicableCommentRequired { get; set; } = true;
    public bool NotApplicableEvidenceRequired { get; set; }
    public bool NotDeterminableCommentRequired { get; set; } = true;
    public bool NotDeterminableEvidenceRequired { get; set; }
    public string? Notes { get; set; }
    public long ConcurrencyVersion { get; set; }
    public AuditInput ToBll() => new(Name, Description, AuditUnitId, CatalogVersionId, new AuditSettings(new Dictionary<Auditarium.Models.Catalog.AuditQuestionResult, ResponseRule>
    {
        [Auditarium.Models.Catalog.AuditQuestionResult.Yes] = new(YesCommentRequired, YesEvidenceRequired),
        [Auditarium.Models.Catalog.AuditQuestionResult.No] = new(NoCommentRequired, NoEvidenceRequired),
        [Auditarium.Models.Catalog.AuditQuestionResult.NotApplicable] = new(NotApplicableCommentRequired, NotApplicableEvidenceRequired),
        [Auditarium.Models.Catalog.AuditQuestionResult.NotDeterminable] = new(NotDeterminableCommentRequired, NotDeterminableEvidenceRequired)
    }), Notes);
    public static AuditInputModel From(AuditDetails item) => new()
    {
        Name = item.Name,
        Description = item.Description,
        AuditUnitId = item.AuditUnitId,
        CatalogVersionId = item.CatalogVersionId,
        Notes = item.Notes,
        ConcurrencyVersion = item.ConcurrencyVersion,
        YesCommentRequired = item.Settings.ResponsePolicy[Auditarium.Models.Catalog.AuditQuestionResult.Yes].CommentRequired,
        YesEvidenceRequired = item.Settings.ResponsePolicy[Auditarium.Models.Catalog.AuditQuestionResult.Yes].EvidenceRequired,
        NoCommentRequired = item.Settings.ResponsePolicy[Auditarium.Models.Catalog.AuditQuestionResult.No].CommentRequired,
        NoEvidenceRequired = item.Settings.ResponsePolicy[Auditarium.Models.Catalog.AuditQuestionResult.No].EvidenceRequired,
        NotApplicableCommentRequired = item.Settings.ResponsePolicy[Auditarium.Models.Catalog.AuditQuestionResult.NotApplicable].CommentRequired,
        NotApplicableEvidenceRequired = item.Settings.ResponsePolicy[Auditarium.Models.Catalog.AuditQuestionResult.NotApplicable].EvidenceRequired,
        NotDeterminableCommentRequired = item.Settings.ResponsePolicy[Auditarium.Models.Catalog.AuditQuestionResult.NotDeterminable].CommentRequired,
        NotDeterminableEvidenceRequired = item.Settings.ResponsePolicy[Auditarium.Models.Catalog.AuditQuestionResult.NotDeterminable].EvidenceRequired
    };
}

public sealed class PublishAuditInputModel
{
    public long ConcurrencyVersion { get; set; }
}
