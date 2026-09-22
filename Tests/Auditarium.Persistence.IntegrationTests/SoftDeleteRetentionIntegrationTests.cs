// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Files;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Features.Audits;
using Auditarium.Bll.Features.Catalog;
using Auditarium.Bll.Features.Jobs;
using Auditarium.Common.Time;
using Auditarium.Dal;
using Auditarium.Models.Catalog;
using Auditarium.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

public sealed class SoftDeleteRetentionIntegrationTests
{
    [Fact]
    public async Task PostgreSql_soft_delete_and_retention_respect_dependencies_and_independent_audit_log_retention()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await VerifyAsync("PostgreSQL", container.GetConnectionString());
    }

    [Fact]
    public async Task SqlServer_soft_delete_and_retention_respect_dependencies_and_independent_audit_log_retention()
    {
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        await VerifyAsync("SqlServer", container.GetConnectionString());
    }

    private static async Task VerifyAsync(string databaseProvider, string connectionString)
    {
        await using var provider = CreateProvider(databaseProvider, connectionString);
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var actor = new TestActor();
        var catalog = new CatalogCommandHandler(db, actor);
        var audits = new AuditCommandHandler(db, actor);

        var purgeDocument = await catalog.Handle(new CreateDocumentCommand(Document("Purge document")), CancellationToken.None);
        var purgeCatalog = await catalog.Handle(new CreateCatalogVersionCommand(purgeDocument.Value!, null), CancellationToken.None);
        var sourceFile = new FileItem
        {
            OriginalFileName = "source.pdf",
            SaveFileName = "stored.pdf",
            Extension = ".pdf",
            SaveFilePath = "documents",
            ContentType = "application/pdf",
            Size = 5,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = 0,
            Checksum = "checksum"
        };
        db.FileItems.Add(sourceFile);
        await db.SaveChangesAsync();
        (await db.CatalogVersions.FindAsync(purgeCatalog.Value))!.SourceFileId = sourceFile.FileId;
        await db.SaveChangesAsync();
        var protectedDocument = await catalog.Handle(new CreateDocumentCommand(Document("Protected document")), CancellationToken.None);
        var protectedCatalog = await catalog.Handle(new CreateCatalogVersionCommand(protectedDocument.Value!, null), CancellationToken.None);
        var recentDocument = await catalog.Handle(new CreateDocumentCommand(Document("Recent document")), CancellationToken.None);
        var purgeUnit = await audits.Handle(new CreateAuditUnitCommand(Unit("Purge unit")), CancellationToken.None);
        var protectedUnit = await audits.Handle(new CreateAuditUnitCommand(Unit("Protected unit")), CancellationToken.None);
        var protectedAudit = await audits.Handle(new CreateAuditCommand(Audit("Protected audit", protectedUnit.Value!, protectedCatalog.Value!)), CancellationToken.None);
        var purgeAudit = await audits.Handle(new CreateAuditCommand(Audit("Purge audit", protectedUnit.Value!, protectedCatalog.Value!)), CancellationToken.None);

        var nonDeletableAudit = (await db.Audits.FindAsync(protectedAudit.Value))!;
        nonDeletableAudit.AuditState = AuditState.Canceled;
        nonDeletableAudit.AuditUnitContext = "{}";
        nonDeletableAudit.StateReason = "canceled";
        await db.SaveChangesAsync();
        var blockedAuditDelete = await audits.Handle(new DeleteAuditCommand(nonDeletableAudit.AuditId, null, nonDeletableAudit.ConcurrencyVersion), CancellationToken.None);
        Assert.Equal("AUDIT.NOT_DELETABLE", Assert.Single(blockedAuditDelete.Errors).Code);

        Assert.True((await catalog.Handle(new DeleteDocumentCommand(purgeDocument.Value!, "expired", (await db.Documents.FindAsync(purgeDocument.Value))!.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        Assert.True((await catalog.Handle(new DeleteDocumentCommand(protectedDocument.Value!, null, (await db.Documents.FindAsync(protectedDocument.Value))!.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        Assert.True((await catalog.Handle(new DeleteDocumentCommand(recentDocument.Value!, null, (await db.Documents.FindAsync(recentDocument.Value))!.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        Assert.True((await audits.Handle(new DeleteAuditUnitCommand(purgeUnit.Value!, null, (await db.AuditUnits.FindAsync(purgeUnit.Value))!.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        Assert.True((await audits.Handle(new DeleteAuditUnitCommand(protectedUnit.Value!, null, (await db.AuditUnits.FindAsync(protectedUnit.Value))!.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        Assert.True((await audits.Handle(new DeleteAuditCommand(purgeAudit.Value!, "obsolete", (await db.Audits.FindAsync(purgeAudit.Value))!.ConcurrencyVersion), CancellationToken.None)).IsSuccess);

        Assert.False(await db.Documents.AnyAsync(x => x.DocumentId == purgeDocument.Value));
        Assert.False((await new CatalogQueryHandler(db).Handle(new GetCatalogVersionQuery(purgeCatalog.Value!), CancellationToken.None)).IsSuccess);
        Assert.False(await db.AuditUnits.AnyAsync(x => x.AuditUnitId == purgeUnit.Value));
        Assert.False(await db.Audits.AnyAsync(x => x.AuditId == purgeAudit.Value));
        Assert.Contains(await db.SystemAuditLogs.ToListAsync(), entry => entry.Action == "DELETED" && entry.ObjectType == nameof(Document) && entry.ObjectId == purgeDocument.Value);

        var old = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await db.Documents.IgnoreQueryFilters().Where(x => x.DeletedAt != null && x.DocumentId != recentDocument.Value).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.DeletedAt, old));
        await db.AuditUnits.IgnoreQueryFilters().Where(x => x.DeletedAt != null).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.DeletedAt, old));
        await db.Audits.IgnoreQueryFilters().Where(x => x.DeletedAt != null).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.DeletedAt, old));
        db.SystemAuditLogs.AddRange(
            new SystemAuditLog { OccurredAt = old, UserId = 0, Action = "UPDATED", ObjectType = nameof(Document), ObjectId = purgeDocument.Value },
            new SystemAuditLog { OccurredAt = old, UserId = 0, Action = "UPDATED", ObjectType = nameof(Document), ObjectId = protectedDocument.Value });
        await db.SaveChangesAsync();

        var clock = new FixedClock(new DateTimeOffset(2026, 9, 22, 0, 0, 0, TimeSpan.Zero));
        var storage = new RecordingFileStorage();
        var retention = new RunRetentionCommandHandler(db, storage, clock, new RetentionOptions(90, true, 365, false), NullLogger<RunRetentionCommandHandler>.Instance);
        Assert.True((await retention.Handle(new RunRetentionCommand(), CancellationToken.None)).IsSuccess);

        Assert.False(await db.Documents.IgnoreQueryFilters().AnyAsync(x => x.DocumentId == purgeDocument.Value));
        Assert.False(await db.FileItems.AnyAsync(x => x.FileId == sourceFile.FileId));
        Assert.Contains(("documents", "stored.pdf"), storage.DeletedFiles);
        Assert.True(await db.Documents.IgnoreQueryFilters().AnyAsync(x => x.DocumentId == protectedDocument.Value));
        Assert.True(await db.Documents.IgnoreQueryFilters().AnyAsync(x => x.DocumentId == recentDocument.Value));
        Assert.False(await db.AuditUnits.IgnoreQueryFilters().AnyAsync(x => x.AuditUnitId == purgeUnit.Value));
        Assert.True(await db.AuditUnits.IgnoreQueryFilters().AnyAsync(x => x.AuditUnitId == protectedUnit.Value));
        Assert.False(await db.Audits.IgnoreQueryFilters().AnyAsync(x => x.AuditId == purgeAudit.Value));
        Assert.True(await db.Audits.IgnoreQueryFilters().AnyAsync(x => x.AuditId == protectedAudit.Value));
        Assert.Contains(await db.SystemAuditLogs.ToListAsync(), entry => entry.OccurredAt == old && entry.ObjectId == purgeDocument.Value);

        retention = new RunRetentionCommandHandler(db, storage, clock, new RetentionOptions(90, false, 365, true), NullLogger<RunRetentionCommandHandler>.Instance);
        Assert.True((await retention.Handle(new RunRetentionCommand(), CancellationToken.None)).IsSuccess);
        Assert.DoesNotContain(await db.SystemAuditLogs.ToListAsync(), entry => entry.OccurredAt == old && entry.ObjectType == nameof(Document) && entry.ObjectId == purgeDocument.Value);
        Assert.Contains(await db.SystemAuditLogs.ToListAsync(), entry => entry.OccurredAt == old && entry.ObjectType == nameof(Document) && entry.ObjectId == protectedDocument.Value);
    }

    private static DocumentInput Document(string title) => new(title, null, null, null, null, DocumentUsageState.Active, null, null);
    private static AuditUnitInput Unit(string name) => new(null, 6, name, null, AuditUnitUsageState.Active, null, null);
    private static AuditInput Audit(string name, long unitId, long catalogId) => new(name, null, unitId, catalogId, null, null);

    private static ServiceProvider CreateProvider(string databaseProvider, string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Auditarium:Database:Provider"] = databaseProvider,
            ["Auditarium:Database:ConnectionString"] = connectionString,
            ["Auditarium:Database:BootstrapTimeoutSeconds"] = "180"
        }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddAuditariumPersistence(configuration);
        return services.BuildServiceProvider();
    }

    private sealed class TestActor : ICurrentActor
    {
        public ActorType Type => ActorType.System;
        public long? UserId => 0;
        public bool IsAuthenticated => true;
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class RecordingFileStorage : IFileStorage
    {
        public List<(string Path, string Name)> DeletedFiles { get; } = [];
        public Task<StoredFile> StoreAsync(Stream content, long maximumSize, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Stream> OpenReadAsync(string saveFilePath, string saveFileName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task DeleteAsync(string saveFilePath, string saveFileName, CancellationToken cancellationToken = default) { DeletedFiles.Add((saveFilePath, saveFileName)); return Task.CompletedTask; }
        public Task<bool> ExistsAsync(string saveFilePath, string saveFileName, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
