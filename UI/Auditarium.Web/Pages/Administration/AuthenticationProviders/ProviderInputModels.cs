// SPDX-License-Identifier: MIT
using System.ComponentModel.DataAnnotations;

namespace Auditarium.Web.Pages.Administration.AuthenticationProviders;

public sealed class CreateProviderInputModel
{
    [Required] public string ProviderKey { get; set; } = string.Empty;
    [Required] public string DisplayName { get; set; } = string.Empty;
}
public sealed class ProviderMetadataInputModel
{
    [Required] public string DisplayName { get; set; } = string.Empty;
    public long ConcurrencyVersion { get; set; }
}
public sealed class ProviderSettingInputModel
{
    [Required] public string Property { get; set; } = string.Empty;
    public string? Value { get; set; }
    public long ConcurrencyVersion { get; set; }
    public long ProviderConcurrencyVersion { get; set; }
}
public sealed class ProviderStateInputModel { public bool IsEnabled { get; set; } public long ConcurrencyVersion { get; set; } }
public sealed class ProviderDeleteInputModel { public bool Confirmed { get; set; } public long ConcurrencyVersion { get; set; } }
