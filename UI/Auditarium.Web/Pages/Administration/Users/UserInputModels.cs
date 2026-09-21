// SPDX-License-Identifier: MIT
using System.ComponentModel.DataAnnotations;

namespace Auditarium.Web.Pages.Administration.Users;

public sealed class UserInputModel
{
    [Required, StringLength(256)] public string Username { get; set; } = string.Empty;
    [Required, StringLength(256)] public string DisplayName { get; set; } = string.Empty;
    [EmailAddress, StringLength(320)] public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public long ConcurrencyVersion { get; set; }
}
public sealed class IdentityInputModel
{
    public long ProviderId { get; set; }
    [Required] public string ExternalId { get; set; } = string.Empty;
}
public sealed class CredentialInputModel
{
    public long IdentityId { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; set; }
}
