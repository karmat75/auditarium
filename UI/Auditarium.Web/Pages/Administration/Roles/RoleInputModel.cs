// SPDX-License-Identifier: MIT
using System.ComponentModel.DataAnnotations;

namespace Auditarium.Web.Pages.Administration.Roles;

public sealed class RoleInputModel
{
    [Required, StringLength(256)] public string Name { get; set; } = string.Empty;
    [StringLength(512)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public long ConcurrencyVersion { get; set; }
}
