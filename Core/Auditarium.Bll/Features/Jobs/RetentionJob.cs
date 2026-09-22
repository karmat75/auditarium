// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Files;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Jobs;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Common.Time;
using Auditarium.Models.Catalog;
using Auditarium.Models.Identity;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auditarium.Bll.Features.Jobs;

public sealed record RetentionOptions(
    int DeletionDays,
    bool DeletionPurgeEnabled,
    int AuditLogDays,
    bool AuditLogPurgeEnabled)
{
    public static RetentionOptions FromConfiguration(IConfiguration configuration)
    {
        var deletionDays = Integer(configuration, "Auditarium:Retention:DeletionDays", 90);
        var auditLogDays = Integer(configuration, "Auditarium:Retention:AuditLogDays", 365);
        if (deletionDays is < 1 or > 36500)
            throw new InvalidOperationException("Auditarium:Retention:DeletionDays must be between 1 and 36500.");
        if (auditLogDays is < 1 or > 36500)
            throw new InvalidOperationException("Auditarium:Retention:AuditLogDays must be between 1 and 36500.");
        return new(
            deletionDays,
            Boolean(configuration, "Auditarium:Retention:DeletionPurgeEnabled", true),
            auditLogDays,
            Boolean(configuration, "Auditarium:Retention:AuditLogPurgeEnabled", false));
    }

    private static int Integer(IConfiguration configuration, string key, int fallback) =>
        configuration[key] is { } value && int.TryParse(value, global::System.Globalization.NumberStyles.Integer, global::System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : configuration[key] is null ? fallback : throw new InvalidOperationException($"{key} must be an integer.");

    private static bool Boolean(IConfiguration configuration, string key, bool fallback) =>
        configuration[key] is { } value && bool.TryParse(value, out var parsed)
            ? parsed
            : configuration[key] is null ? fallback : throw new InvalidOperationException($"{key} must be a boolean.");
}

[RequiresPermission("Maintenance.Retention.Execute")]
public sealed record RunRetentionCommand : IJobExecutionRequest;

public sealed class RetentionJobExecutionRequestFactory : IJobExecutionRequestFactory
{
    public bool TryCreate(string jobKey, out IJobExecutionRequest? request)
    {
        request = string.Equals(jobKey, JobKeys.Retention, StringComparison.Ordinal) ? new RunRetentionCommand() : null;
        return request is not null;
    }
}

public sealed class RunRetentionCommandHandler(
    IAuditariumDbContext db,
    IFileStorage fileStorage,
    IClock clock,
    RetentionOptions options,
    ILogger<RunRetentionCommandHandler> logger) : IRequestHandler<RunRetentionCommand, Result>
{
    public async ValueTask<Result> Handle(RunRetentionCommand command, CancellationToken ct)
    {
        var auditsPurged = 0;
        var documentsPurged = 0;
        var auditUnitsPurged = 0;
        var auditLogsPurged = 0;
        var physicalFiles = new List<(string Path, string Name)>();

        if (options.DeletionPurgeEnabled)
        {
            var cutoff = clock.UtcNow.AddDays(-options.DeletionDays);
            await using var transaction = await db.BeginTransactionAsync(ct);
            try
            {
                auditsPurged = await PurgeAuditsAsync(cutoff, ct);
                (documentsPurged, physicalFiles) = await PurgeDocumentsAsync(cutoff, ct);
                auditUnitsPurged = await PurgeAuditUnitsAsync(cutoff, ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }

            foreach (var file in physicalFiles)
                await fileStorage.DeleteAsync(file.Path, file.Name, ct);
        }

        if (options.AuditLogPurgeEnabled)
            auditLogsPurged = await PurgeAuditLogAsync(clock.UtcNow.AddDays(-options.AuditLogDays), ct);

        logger.LogInformation(
            "Retention completed: {AuditsPurged} audits, {DocumentsPurged} documents, {AuditUnitsPurged} audit units and {AuditLogsPurged} audit-log entries purged.",
            auditsPurged,
            documentsPurged,
            auditUnitsPurged,
            auditLogsPurged);
        return Result.Success();
    }

    private async Task<int> PurgeAuditsAsync(DateTimeOffset cutoff, CancellationToken ct)
    {
        var total = 0;
        while (true)
        {
            var candidates = await db.Audits.IgnoreQueryFilters()
                .Where(audit => audit.DeletedAt != null && audit.DeletedAt <= cutoff)
                .Where(audit => !db.Audits.IgnoreQueryFilters().Any(other => other.OriginAuditId == audit.AuditId))
                .ToListAsync(ct);
            if (candidates.Count == 0) return total;
            db.Audits.RemoveRange(candidates);
            await db.SaveChangesAsync(ct);
            total += candidates.Count;
        }
    }

    private async Task<(int Count, List<(string Path, string Name)> Files)> PurgeDocumentsAsync(DateTimeOffset cutoff, CancellationToken ct)
    {
        var candidates = await db.Documents.IgnoreQueryFilters()
            .Where(document => document.DeletedAt != null && document.DeletedAt <= cutoff)
            .Where(document => !db.Audits.IgnoreQueryFilters().Any(audit =>
                db.CatalogVersions.Where(catalog => catalog.DocumentId == document.DocumentId)
                    .Select(catalog => catalog.CatalogVersionId)
                    .Contains(audit.CatalogVersionId)))
            .ToListAsync(ct);
        if (candidates.Count == 0) return (0, []);

        var documentIds = candidates.Select(x => x.DocumentId).ToArray();
        var fileIds = await db.CatalogVersions
            .Where(catalog => documentIds.Contains(catalog.DocumentId) && catalog.SourceFileId != null)
            .Select(catalog => catalog.SourceFileId!.Value)
            .Distinct()
            .ToListAsync(ct);
        var files = await db.FileItems
            .Where(file => fileIds.Contains(file.FileId))
            .Where(file => !db.CatalogVersions.Any(catalog => !documentIds.Contains(catalog.DocumentId) && catalog.SourceFileId == file.FileId))
            .ToListAsync(ct);

        db.Documents.RemoveRange(candidates);
        db.FileItems.RemoveRange(files);
        await db.SaveChangesAsync(ct);
        return (candidates.Count, files.Select(file => (file.SaveFilePath, file.SaveFileName)).ToList());
    }

    private async Task<int> PurgeAuditUnitsAsync(DateTimeOffset cutoff, CancellationToken ct)
    {
        var total = 0;
        while (true)
        {
            var candidates = await db.AuditUnits.IgnoreQueryFilters()
                .Where(unit => unit.DeletedAt != null && unit.DeletedAt <= cutoff)
                .Where(unit => !db.AuditUnits.IgnoreQueryFilters().Any(child => child.ParentAuditUnitId == unit.AuditUnitId))
                .Where(unit => !db.Audits.IgnoreQueryFilters().Any(audit => audit.AuditUnitId == unit.AuditUnitId))
                .ToListAsync(ct);
            if (candidates.Count == 0) return total;
            db.AuditUnits.RemoveRange(candidates);
            await db.SaveChangesAsync(ct);
            total += candidates.Count;
        }
    }

    private async Task<int> PurgeAuditLogAsync(DateTimeOffset cutoff, CancellationToken ct)
    {
        var candidates = await db.SystemAuditLogs
            .Where(entry => entry.OccurredAt <= cutoff && entry.ObjectId != null)
            .ToListAsync(ct);
        var purge = new List<SystemAuditLog>();
        foreach (var entry in candidates)
            if (!await OriginExistsAsync(entry.ObjectType, entry.ObjectId!.Value, ct))
                purge.Add(entry);
        db.SystemAuditLogs.RemoveRange(purge);
        await db.SaveChangesAsync(ct);
        return purge.Count;
    }

    private async Task<bool> OriginExistsAsync(string objectType, long objectId, CancellationToken ct) => objectType switch
    {
        nameof(Document) => await db.Documents.IgnoreQueryFilters().AnyAsync(x => x.DocumentId == objectId, ct),
        nameof(CatalogVersion) => await db.CatalogVersions.AnyAsync(x => x.CatalogVersionId == objectId, ct),
        nameof(DocumentElement) => await db.DocumentElements.AnyAsync(x => x.ElementId == objectId, ct),
        nameof(DocumentElementWeight) => await db.DocumentElementWeights.AnyAsync(x => x.ElementId == objectId, ct),
        nameof(Question) => await db.Questions.AnyAsync(x => x.QuestionId == objectId, ct),
        nameof(FileItem) => await db.FileItems.AnyAsync(x => x.FileId == objectId, ct),
        nameof(AuditUnit) => await db.AuditUnits.IgnoreQueryFilters().AnyAsync(x => x.AuditUnitId == objectId, ct),
        nameof(Audit) => await db.Audits.IgnoreQueryFilters().AnyAsync(x => x.AuditId == objectId, ct),
        nameof(AuditDocumentElement) => await db.AuditDocumentElements.AnyAsync(x => x.AuditDocumentElementId == objectId, ct),
        nameof(AuditQuestion) => await db.AuditQuestions.AnyAsync(x => x.AuditQuestionId == objectId, ct),
        _ => true
    };
}
