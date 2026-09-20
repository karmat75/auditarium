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
        _ => $"Die Aktion konnte nicht ausgeführt werden ({error.Code})."
    };
}
