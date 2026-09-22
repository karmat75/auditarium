// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Settings;

public sealed record SystemRoleDefinition(string Key, string Name, string Description, IReadOnlySet<string> Permissions);

public static class SystemRoleDefinitions
{
    private static IReadOnlySet<string> Set(params string[] keys) => new HashSet<string>(keys, StringComparer.Ordinal);

    public static IReadOnlyList<SystemRoleDefinition> All { get; } =
    [
        new("SYSTEM_ADMIN", "System Administrator", "Verwaltet Benutzer, Rollen, Authentifizierung und technische Einstellungen.", Set("Users.Manage", "Roles.Manage", "Authentication.Manage", "Settings.Manage", "Maintenance.Jobs.Execute", "AuditHistory.Read")),
        new("AUDIT_MANAGER", "Audit Manager", "Verwaltet Regelwerke, Prüfeinheiten und Audits.", Set("Documents.Manage", "AuditUnits.Manage", "Audits.Create", "Audits.UpdateDraft", "Audits.Publish", "Audits.Assign", "Audits.Cancel", "Audits.Reopen", "Audits.Read", "Audits.ReadFinalized", "Audits.Evaluate", "Reports.Export", "AuditHistory.Read")),
        new("AUDITOR", "Auditor", "Führt Audits durch.", Set("Audits.Claim", "Audits.ReleaseOwn", "Audits.Answer", "Audits.Finalize", "Audits.Read", "Audits.ReadFinalized")),
        new("REVIEWER", "Reviewer", "Prüft und wertet Auditergebnisse aus.", Set("Audits.Read", "Audits.ReadFinalized", "Audits.Evaluate", "Reports.Export", "AuditHistory.Read")),
        new("VIEWER", "Viewer", "Liest abgeschlossene Inhalte und Reports.", Set("Audits.ReadFinalized", "Reports.Export")),
        new("SYSTEM_INTERNAL", "System Internal", "Minimalrolle für kontrollierte interne Prozesse.", Set())
    ];
}
