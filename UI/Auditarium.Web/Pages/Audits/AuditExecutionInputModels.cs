// SPDX-License-Identifier: MIT
using Auditarium.Models.Catalog;

namespace Auditarium.Web.Pages.Audits;

public class AuditActionInputModel
{
    public long ConcurrencyVersion { get; set; }
}

public sealed class AssignmentInputModel : AuditActionInputModel
{
    public long? UserId { get; set; }
}

public sealed class ReasonInputModel : AuditActionInputModel
{
    public string Reason { get; set; } = string.Empty;
}

public sealed class AnswerAuditQuestionInputModel : AuditActionInputModel
{
    public AuditQuestionResult Result { get; set; }
    public string? Comment { get; set; }
    public string? Evidence { get; set; }
}
