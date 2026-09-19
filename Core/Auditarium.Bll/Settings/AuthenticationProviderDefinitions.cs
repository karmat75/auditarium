// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Settings;

public sealed record AuthenticationProviderDefinition(string ProviderType, bool AllowMultipleInstances, bool SystemManaged, IReadOnlySet<string> SupportedProvisioningModes);
public static class AuthenticationProviderDefinitions
{
    public static readonly IReadOnlyList<AuthenticationProviderDefinition> All =
    [
        new("LOCAL", false, true, new HashSet<string>(StringComparer.Ordinal)),
        new("API", false, true, new HashSet<string>(StringComparer.Ordinal)),
        new("LDAP", true, false, new HashSet<string>(["EXISTING_ONLY", "CREATE_INACTIVE", "CREATE_ACTIVE"], StringComparer.Ordinal))
    ];
}
