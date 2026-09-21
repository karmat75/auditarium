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
