// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Settings;

using Auditarium.Bll.Jobs;

public sealed record SettingDefinition(
    string Key,
    string DataType,
    string DefaultValue,
    bool UiEditable,
    Func<string, bool> IsValid,
    bool Secret = false,
    bool RestartRequired = false,
    string Category = "Allgemein",
    string? DisplayName = null,
    int DisplayOrder = 0,
    IReadOnlyList<string>? AllowedValues = null);
public static class SettingDefinitions
{
    public static IReadOnlyList<SettingDefinition> All { get; } =
    [
        new("Security:LocalPassword:MinimumLength", "int", "15", true, v => int.TryParse(v, out var n) && n is >= 12 and <= 128, Category: "Lokale Passwörter", DisplayName: "Mindestlänge", DisplayOrder: 10),
        new("Security:LocalPassword:MaximumLength", "int", "128", true, v => int.TryParse(v, out var n) && n is >= 12 and <= 1024, Category: "Lokale Passwörter", DisplayName: "Maximallänge", DisplayOrder: 20),
        new("Security:LocalLockout:FailedAttempts", "int", "5", true, v => int.TryParse(v, out var n) && n is >= 1 and <= 20, Category: "Lokale Kontosperre", DisplayName: "Erlaubte Fehlversuche", DisplayOrder: 30),
        new("Security:LocalLockout:WindowMinutes", "int", "15", true, v => int.TryParse(v, out var n) && n is >= 1 and <= 1440, Category: "Lokale Kontosperre", DisplayName: "Beobachtungsfenster (Minuten)", DisplayOrder: 40),
        new("Security:LocalLockout:DurationMinutes", "int", "15", true, v => int.TryParse(v, out var n) && n is >= 1 and <= 1440, Category: "Lokale Kontosperre", DisplayName: "Sperrdauer (Minuten)", DisplayOrder: 50),
        new("Security:ApiCredentials:DefaultLifetimeDays", "int", "180", true, v => int.TryParse(v, out var n) && n is >= 1 and <= 3650, Category: "API-Credentials", DisplayName: "Standardlaufzeit (Tage)", DisplayOrder: 60),
        new("Security:ApiCredentials:MaximumActiveCredentials", "int", "5", true, v => int.TryParse(v, out var n) && n is >= 1 and <= 50, Category: "API-Credentials", DisplayName: "Maximal aktive Credentials", DisplayOrder: 70),
        new("Files:OriginalDocuments:MaxUploadSize", "int", "104857600", false, v => int.TryParse(v, out var n) && n is >= 1_048_576 and <= 1_073_741_824, RestartRequired: true, Category: "Dateien", DisplayName: "Maximale Uploadgröße", DisplayOrder: 80),
        new("Files:OriginalDocuments:MaxDownloadSize", "int", "104857600", false, v => int.TryParse(v, out var n) && n is >= 1_048_576 and <= 1_073_741_824, RestartRequired: true, Category: "Dateien", DisplayName: "Maximale Downloadgröße", DisplayOrder: 90),
        .. JobSettingDefinitions.All
    ];
}

public static class LdapProviderSettingDefinitions
{
    public static IReadOnlyList<SettingDefinition> For(string providerKey)
    {
        var prefix = $"Authentication:Providers:{providerKey}:";
        return
        [
            Text("Host", "Host", true, 10),
            Int("Port", "Port", "389", value => value is >= 1 and <= 65535, 20),
            Choice("TlsMode", "TLS-Modus", "None", ["None", "StartTls", "Ldaps"], 30),
            Int("ConnectionTimeoutSeconds", "Verbindungs-Timeout (Sekunden)", "15", value => value is >= 1 and <= 300, 40),
            Text("BindUser", "Bind-Benutzer", false, 50),
            new(prefix + "BindPassword", "secret", string.Empty, true, _ => true, true, Category: "LDAP", DisplayName: "Bind-Passwort", DisplayOrder: 60),
            Text("BaseDn", "Base DN", true, 70),
            Text("LoginAttribute", "Login-/Suchattribut", false, 80, "uid"),
            Text("SearchFilter", "Suchfilter", false, 90, "(objectClass=person)"),
            Text("StableExternalIdAttribute", "Stabile externe ID", false, 100, "entryUUID"),
            Text("DisplayNameAttribute", "Anzeigename-Attribut", false, 110, "cn"),
            Text("EmailAttribute", "E-Mail-Attribut", false, 120, "mail"),
            Choice("ProvisioningMode", "Provisioning-Modus", "EXISTING_ONLY", ["EXISTING_ONLY", "CREATE_INACTIVE", "CREATE_ACTIVE"], 130),
            Text("AutoProvisionRoles", "Automatische Rollen", false, 140),
            Text("DomainPrefixes", "Domain-/NetBIOS-Präfixe", false, 150),
            Text("UpnSuffixes", "UPN-Suffixe", false, 160)
        ];

        SettingDefinition Text(string property, string name, bool required, int order, string defaultValue = "") =>
            new(prefix + property, "string", defaultValue, true, value => !required || !string.IsNullOrWhiteSpace(value), Category: "LDAP", DisplayName: name, DisplayOrder: order);
        SettingDefinition Int(string property, string name, string defaultValue, Func<int, bool> valid, int order) =>
            new(prefix + property, "int", defaultValue, true, value => int.TryParse(value, out var parsed) && valid(parsed), Category: "LDAP", DisplayName: name, DisplayOrder: order);
        SettingDefinition Choice(string property, string name, string defaultValue, IReadOnlyList<string> choices, int order) =>
            new(prefix + property, "string", defaultValue, true, value => choices.Contains(value, StringComparer.OrdinalIgnoreCase), Category: "LDAP", DisplayName: name, DisplayOrder: order, AllowedValues: choices);
    }
}
