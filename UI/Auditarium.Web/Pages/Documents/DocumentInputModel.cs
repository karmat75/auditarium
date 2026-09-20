// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Catalog;
using Auditarium.Models.Catalog;

namespace Auditarium.Web.Pages.Documents;

public sealed class DocumentInputModel
{
    public string Title { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? Version { get; set; }
    public DateOnly? PublicationDate { get; set; }
    public string? Source { get; set; }
    public DocumentUsageState UsageState { get; set; } = DocumentUsageState.Active;
    public string? UsageStateReason { get; set; }
    public string? Notes { get; set; }
    public long ConcurrencyVersion { get; set; }
    public DocumentInput ToBll() => new(Title, Publisher, Version, PublicationDate, Source, UsageState, UsageStateReason, Notes);
    public static DocumentInputModel From(DocumentDetails document) => new() { Title = document.Title, Publisher = document.Publisher, Version = document.Version, PublicationDate = document.PublicationDate, Source = document.Source, UsageState = document.UsageState, UsageStateReason = document.UsageStateReason, Notes = document.Notes, ConcurrencyVersion = document.ConcurrencyVersion };
}
