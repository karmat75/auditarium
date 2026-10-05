namespace Auditarium.Web.Components;

public sealed record StatusPresentation(string Label, string Tone);

public static class StatusPresentations
{
    public static StatusPresentation Resolve(object? value)
    {
        var status = value?.ToString()?.ToUpperInvariant();

        return status switch
        {
            "DRAFT" => new("Entwurf", "neutral"),
            "READY" => new("Bereit", "info"),
            "INPROGRESS" or "IN_PROGRESS" => new("In Bearbeitung", "warning"),
            "FINALIZED" => new("Abgeschlossen", "success"),
            "CANCELED" or "CANCELLED" => new("Abgebrochen", "danger"),
            "ACTIVE" => new("Aktiv", "success"),
            "INACTIVE" => new("Inaktiv", "neutral"),
            "DEPRECATED" => new("Veraltet", "warning"),
            _ => new(value?.ToString() ?? "Unbekannt", "neutral")
        };
    }
}
