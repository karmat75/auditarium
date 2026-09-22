// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Settings;

public sealed record PermissionDefinition(string Key, string Description);

public static class PermissionDefinitions
{
    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        new("Users.Manage", "Benutzer verwalten"),
        new("Roles.Manage", "Rollen verwalten"),
        new("Authentication.Manage", "Authentifizierung konfigurieren"),
        new("Settings.Manage", "Systemeinstellungen verwalten"),
        new("Maintenance.Jobs.Execute", "Wartungsjobs ausführen"),
        new("Documents.Manage", "Dokumente und Kataloge verwalten"),
        new("AuditUnits.Manage", "Prüfeinheiten verwalten"),
        new("Audits.Create", "Audits anlegen"),
        new("Audits.UpdateDraft", "Audit-Entwürfe bearbeiten"),
        new("Audits.Publish", "Audits veröffentlichen"),
        new("Audits.Assign", "Auditor-Zuweisungen verwalten"),
        new("Audits.Cancel", "Audits abbrechen"),
        new("Audits.Reopen", "Abgebrochene Audits wieder öffnen"),
        new("Audits.Claim", "Freie Audits übernehmen"),
        new("Audits.ReleaseOwn", "Eigene Auditor-Zuweisung freigeben"),
        new("Audits.Answer", "Auditfragen beantworten"),
        new("Audits.Finalize", "Audits finalisieren"),
        new("Audits.Read", "Laufende Audits lesen"),
        new("Audits.ReadFinalized", "Finalisierte Audits lesen"),
        new("Audits.Evaluate", "Audit-Ergebnisse auswerten"),
        new("Reports.Export", "Reports erstellen und exportieren"),
        new("AuditHistory.Read", "System- und Audit-Historie lesen")
    ];
}
