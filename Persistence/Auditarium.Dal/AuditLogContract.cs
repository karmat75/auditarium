// SPDX-License-Identifier: MIT
namespace Auditarium.Dal;

/// <summary>Stable, centrally validated audit-log vocabulary.</summary>
internal static class AuditLogContract
{
    private static readonly HashSet<string> Actions = ["CREATED", "UPDATED", "PURGED", "STATE_CHANGED", "LOGIN", "LOGIN_FAILED", "LOGOUT", "ACCESS_DENIED", "JOB_TRIGGERED"];
    private static readonly HashSet<string> ObjectTypes = ["User", "AuthenticationProvider", "UserIdentity", "LocalCredential", "ApiCredential", "Permission", "Role", "RolePermission", "UserRole", "ApplicationSetting", "Document", "CatalogVersion", "DocumentElement", "DocumentElementWeight", "Question", "QuestionScopeType", "FileItem", "AuditUnit", "Audit", "AuditDocumentElement", "AuditQuestion", "Authentication", "Authorization", "MaintenanceJob"];

    public static void Validate(string action, string objectType)
    {
        if (!Actions.Contains(action)) throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown audit-log action.");
        if (!ObjectTypes.Contains(objectType)) throw new ArgumentOutOfRangeException(nameof(objectType), objectType, "Unknown audit-log object type.");
    }
}
