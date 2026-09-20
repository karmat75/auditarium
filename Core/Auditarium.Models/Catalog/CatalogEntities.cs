// SPDX-License-Identifier: MIT
namespace Auditarium.Models.Catalog;

public enum DocumentUsageState { Active, Deprecated }
public enum CatalogState { Draft, Ready }

public sealed class Document
{
    public long DocumentId { get; set; }
    public required string Title { get; set; }
    public string? Publisher { get; set; }
    public string? Version { get; set; }
    public DateOnly? PublicationDate { get; set; }
    public string? Source { get; set; }
    public DocumentUsageState UsageState { get; set; } = DocumentUsageState.Active;
    public string? UsageStateReason { get; set; }
    public string? Notes { get; set; }
    public long ConcurrencyVersion { get; set; } = 1;
}

public sealed class CatalogVersion
{
    public long CatalogVersionId { get; set; }
    public long DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public CatalogState CatalogState { get; set; } = CatalogState.Draft;
    public int DraftRevision { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public string? Notes { get; set; }
    public long? SourceFileId { get; set; }
    public long ConcurrencyVersion { get; set; } = 1;
}

public sealed class DocumentElement
{
    public long ElementId { get; set; }
    public long CatalogVersionId { get; set; }
    public long? ParentElementId { get; set; }
    public string? Title { get; set; }
    public string? Text { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
}

public sealed class DocumentElementWeight { public long ElementId { get; set; } public int Weight { get; set; } }
public sealed class Question { public long QuestionId { get; set; } public long ElementId { get; set; } public int SortOrder { get; set; } public required string Text { get; set; } public string? VerificationHint { get; set; } public string? EvidenceHint { get; set; } public string? Notes { get; set; } }
public sealed class ScopeType { public long ScopeTypeId { get; set; } public required string Key { get; set; } public required string Name { get; set; } public required string Description { get; set; } }
public sealed class QuestionScopeType { public long QuestionId { get; set; } public long ScopeTypeId { get; set; } }
public sealed class FileItem { public long FileId { get; set; } public required string OriginalFileName { get; set; } public required string SaveFileName { get; set; } public required string Extension { get; set; } public required string SaveFilePath { get; set; } public required string ContentType { get; set; } public long Size { get; set; } public DateTimeOffset CreatedAt { get; set; } public long CreatedBy { get; set; } public required string Checksum { get; set; } }
