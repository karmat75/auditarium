// SPDX-License-Identifier: MIT
namespace Auditarium.Web.Pages;

public sealed class SoftDeleteInputModel
{
    public string? Reason { get; set; }
    public long ConcurrencyVersion { get; set; }
}
