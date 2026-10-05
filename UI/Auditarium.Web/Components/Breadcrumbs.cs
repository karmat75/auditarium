namespace Auditarium.Web.Components;

public sealed record BreadcrumbItem(string Label, string? Href = null);

public static class Breadcrumbs
{
    private static readonly IReadOnlyDictionary<string, BreadcrumbItem> Segments = new Dictionary<string, BreadcrumbItem>(StringComparer.OrdinalIgnoreCase)
    {
        ["users"] = new("Benutzer", "/administration/users"),
        ["roles"] = new("Rollen", "/administration/roles"),
        ["authentication-providers"] = new("Authentication Provider", "/administration/authentication-providers"),
        ["settings"] = new("Einstellungen", "/administration/settings"),
        ["jobs"] = new("Jobs", "/administration/jobs"),
        ["audits"] = new("Audits", "/audits"),
        ["audit-units"] = new("Audit Units", "/audit-units"),
        ["documents"] = new("Regelwerke", "/documents"),
        ["catalog-versions"] = new("Katalogversionen"),
        ["analysis"] = new("Auswertung", "/analysis"),
        ["exports"] = new("CSV-Export", "/exports")
    };

    public static IReadOnlyList<BreadcrumbItem> For(string path, string pageTitle)
    {
        var result = new List<BreadcrumbItem> { new("Auditarium", "/") };
        foreach (var segment in path.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1))
        {
            if (Segments.TryGetValue(segment, out var item) && result.All(existing => !string.Equals(existing.Label, item.Label, StringComparison.Ordinal)))
            {
                result.Add(item);
            }
        }

        if (!string.Equals(result[^1].Label, pageTitle, StringComparison.Ordinal)) result.Add(new(pageTitle));
        return result;
    }
}
