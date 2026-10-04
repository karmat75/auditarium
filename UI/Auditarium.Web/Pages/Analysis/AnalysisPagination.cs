// SPDX-License-Identifier: MIT
using Auditarium.Models.Catalog;

namespace Auditarium.Web.Pages.Analysis;

public sealed record AnalysisPagination(int PageNumber, int PageSize, long TotalCount, IDictionary<string, string> RouteValues)
{
    public long PageCount => Math.Max(1, (TotalCount + PageSize - 1) / PageSize);
}

public static class AnalysisRoutes
{
    public static IDictionary<string, string> For(AnalysisFilterInputModel filter, AnalysisView? view = null, bool? descending = null, TimelineGroup? groupBy = null) =>
        new Dictionary<string, string?>
        {
            ["Filter.CreatedFrom"] = filter.CreatedFrom?.ToString("O"),
            ["Filter.CreatedTo"] = filter.CreatedTo?.ToString("O"),
            ["Filter.ScopeTypeId"] = filter.ScopeTypeId?.ToString(),
            ["Filter.AuditUnitId"] = filter.AuditUnitId?.ToString(),
            ["Filter.DocumentId"] = filter.DocumentId?.ToString(),
            ["Filter.CatalogVersionId"] = filter.CatalogVersionId?.ToString(),
            ["Filter.AuditState"] = filter.AuditState?.ToString(),
            ["View"] = view?.ToString(),
            ["Descending"] = descending?.ToString(),
            ["GroupBy"] = groupBy?.ToString()
        }.Where(pair => pair.Value is not null).ToDictionary(pair => pair.Key, pair => pair.Value!);
}
