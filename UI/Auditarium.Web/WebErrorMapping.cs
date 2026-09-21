// SPDX-License-Identifier: MIT
using Auditarium.Common.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Auditarium.Web;

public static class WebErrorMapping
{
    public static void ApplyTo(this IAppResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            var key = string.IsNullOrWhiteSpace(error.Target) ? string.Empty : error.Target;
            modelState.AddModelError(key, MessageFor(error));
        }
    }

    private static string MessageFor(AppError error) => error.Code switch
    {
        "AUTHENTICATION.CREDENTIALS_REQUIRED" => "Benutzername und Passwort sind erforderlich.",
        "AUTHENTICATION.FAILED" => "Anmeldung nicht möglich.",
        "AUTHENTICATION.PASSWORD_REQUIRED" => "Aktuelles und neues Passwort sind erforderlich.",
        "AUTHENTICATION.PASSWORD_CHANGE_FAILED" => "Passwortwechsel nicht möglich.",
        "AUTHENTICATION.PASSWORD_CHANGE_REQUIRED" => "Ändern Sie zuerst Ihr Passwort.",
        "AUTHORIZATION.FORBIDDEN" => "Sie verfügen nicht über die erforderliche Berechtigung.",
        "AUTHENTICATION.REQUIRED" => "Melden Sie sich an, um fortzufahren.",
        "USER.CONCURRENCY_CONFLICT" => "Der Benutzer wurde inzwischen geändert. Laden Sie die Seite neu.",
        "ROLE.CONCURRENCY_CONFLICT" => "Die Rolle wurde inzwischen geändert. Laden Sie die Seite neu.",
        "ROLE.SYSTEM_MANAGED" => "Systemverwaltete Rollen sind schreibgeschützt.",
        "USER.SYSTEM_MANAGED_ROLES" => "Die Rollenzuordnung dieses systemverwalteten Benutzers ist geschützt.",
        "AUTH_PROVIDER.SYSTEM_MANAGED" => "Dieser Authentication Provider ist systemverwaltet.",
        "AUTH_PROVIDER.HAS_DEPENDENCIES" => "Der Provider besitzt abhängige Identitäten und kann nicht gelöscht werden.",
        "AUTH_PROVIDER.CONCURRENCY_CONFLICT" => "Der Provider wurde inzwischen geändert. Laden Sie die Seite neu.",
        "AUTH_PROVIDER.SETTING_CONCURRENCY_CONFLICT" => "Die Provider-Konfiguration wurde inzwischen geändert. Laden Sie die Seite neu.",
        "AUTH_PROVIDER.SETTING_EXTERNALLY_OVERRIDDEN" => "Diese Eigenschaft wird extern überschrieben und ist schreibgeschützt.",
        "LDAP.CONNECTION_TEST_FAILED" => "Der LDAP-Verbindungstest ist fehlgeschlagen.",
        "SETTING.CONCURRENCY_CONFLICT" => "Das Setting wurde inzwischen geändert. Laden Sie die Seite neu.",
        "SETTING.EXTERNALLY_OVERRIDDEN" => "Das Setting wird extern überschrieben und ist schreibgeschützt.",
        "API_CREDENTIAL.MAXIMUM_ACTIVE_REACHED" => "Die maximale Anzahl aktiver API-Credentials ist erreicht.",
        "CATALOG.QUESTION_SCOPE_REQUIRED" => "Jede Frage benötigt für READY mindestens einen gültigen Scope Type.",
        "CATALOG.QUESTIONED_ELEMENT_TEXT_REQUIRED" => "Ein Element mit Fragen benötigt einen Text.",
        "CATALOG.PARENT_INVALID" => "Die gewählte Hierarchie ist ungültig oder zyklisch.",
        "CATALOG.NOT_DRAFT" => "Der Katalog ist nicht mehr bearbeitbar, weil er nicht im Zustand DRAFT ist.",
        "CATALOG.CONCURRENCY_CONFLICT" => "Der Katalog wurde inzwischen geändert. Laden Sie die Seite neu.",
        "AUDIT_UNIT.CONCURRENCY_CONFLICT" => "Die Audit Unit wurde inzwischen geändert. Laden Sie die Seite neu.",
        "AUDIT.CONCURRENCY_CONFLICT" => "Das Audit wurde inzwischen geändert. Laden Sie die Seite neu.",
        "AUDIT.NOT_DRAFT" => "Das Audit ist nicht mehr im Zustand DRAFT und kann nicht geändert werden.",
        "AUDIT.QUESTIONS_REQUIRED" => "Die Konfiguration enthält keine passenden Auditfragen und kann nicht veröffentlicht werden.",
        "AUDIT.AUDIT_UNIT_INACTIVE" => "Die gewählte Audit Unit ist für neue Audits nicht aktiv.",
        "AUDIT.CATALOG_NOT_USABLE" => "Die Katalogversion ist nicht READY oder ihr Dokument ist nicht aktiv.",
        "IMPORT.BASE_REVISION_MISMATCH" => "Das Importpaket basiert nicht auf der aktuellen DRAFT-Revision.",
        _ => $"Die Aktion konnte nicht ausgeführt werden ({error.Code})."
    };
}
