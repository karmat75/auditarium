// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Features.Catalog;
using Auditarium.Dal;
using Auditarium.Models.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

public sealed class CatalogWorkflowIntegrationTests
{
    [Fact]
    public async Task Catalog_lifecycle_rejects_a_stale_concurrency_version()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var handler = new CatalogCommandHandler(db, new TestActor());
        var document = await handler.Handle(new CreateDocumentCommand(new("Regelwerk", null, null, null, null, DocumentUsageState.Active, null, null)), CancellationToken.None);
        var catalog = await handler.Handle(new CreateCatalogVersionCommand(document.Value!, null), CancellationToken.None);

        var result = await handler.Handle(new SetCatalogReadyCommand(catalog.Value!, 0, true), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("CATALOG.CONCURRENCY_CONFLICT", Assert.Single(result.Errors).Code);
        Assert.Equal(CatalogState.Draft, (await db.CatalogVersions.FindAsync(catalog.Value))!.CatalogState);
    }

    [Fact]
    public async Task Import_apply_is_provider_neutral_on_sql_server()
    {
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        await VerifyValidImportApplyAsync("SqlServer", container.GetConnectionString());
    }

    [Fact]
    public async Task Import_apply_is_provider_neutral_on_postgresql()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await VerifyValidImportApplyAsync("PostgreSQL", container.GetConnectionString());
    }

    [Fact]
    public async Task Import_apply_rolls_back_every_change_when_a_database_write_fails()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var catalogHandler = new CatalogCommandHandler(db, new TestActor());
        var document = await catalogHandler.Handle(new CreateDocumentCommand(new("Regelwerk", null, null, null, null, DocumentUsageState.Active, null, null)), CancellationToken.None);
        var catalog = await catalogHandler.Handle(new CreateCatalogVersionCommand(document.Value!, null), CancellationToken.None);
        await db.Database.ExecuteSqlRawAsync("CREATE FUNCTION auditarium.reject_import_question() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'reject import'; END; $$; CREATE TRIGGER reject_import_question BEFORE INSERT ON auditarium.questions FOR EACH ROW EXECUTE FUNCTION auditarium.reject_import_question();");

        await Assert.ThrowsAsync<DbUpdateException>(() => new CatalogImportHandler(db).Handle(new ApplyCatalogImportCommand(ValidPackage(catalog.Value!, 1), ImportApplyMode.Full, []), CancellationToken.None).AsTask());
        db.ChangeTracker.Clear();

        Assert.Empty(db.DocumentElements.Where(x => x.CatalogVersionId == catalog.Value));
        Assert.Empty(db.Questions);
        Assert.Equal(1, (await db.CatalogVersions.FindAsync(catalog.Value))!.DraftRevision);
    }

    [Fact]
    public async Task Import_validates_without_writing_applies_only_valid_roots_and_protects_the_draft_revision()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var catalogHandler = new CatalogCommandHandler(db, new TestActor());
        var document = await catalogHandler.Handle(new CreateDocumentCommand(new("Regelwerk", null, null, null, null, DocumentUsageState.Active, null, null)), CancellationToken.None);
        var catalog = await catalogHandler.Handle(new CreateCatalogVersionCommand(document.Value!, null), CancellationToken.None);
        var importHandler = new CatalogImportHandler(db);
        var package = Package(catalog.Value!, 1);

        var report = await importHandler.Handle(new ValidateCatalogImportCommand(package), CancellationToken.None);
        Assert.True(report.IsSuccess);
        Assert.Equal(ImportStatus.PartiallyValid, report.Value!.Status);
        Assert.Empty(db.DocumentElements);
        Assert.Equal(1, (await db.CatalogVersions.FindAsync(catalog.Value))!.DraftRevision);

        var applied = await importHandler.Handle(new ApplyCatalogImportCommand(package, ImportApplyMode.Partial, ["valid-root"]), CancellationToken.None);
        Assert.True(applied.IsSuccess);
        var root = Assert.Single(db.DocumentElements.Where(x => x.CatalogVersionId == catalog.Value));
        Assert.Equal("Valid root", root.Title);
        Assert.Single(db.Questions.Where(x => x.ElementId == root.ElementId));
        Assert.Equal(2, (await db.CatalogVersions.FindAsync(catalog.Value))!.DraftRevision);

        var stale = await importHandler.Handle(new ValidateCatalogImportCommand(package), CancellationToken.None);
        Assert.Equal(ImportStatus.Rejected, stale.Value!.Status);
        Assert.Contains(stale.Value.Errors, x => x.Code == "IMPORT.BASE_REVISION_MISMATCH");
        Assert.Single(db.DocumentElements);
    }

    [Fact]
    public async Task Draft_workflow_validates_publishes_copies_and_preserves_the_ready_content()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var actor = new TestActor();
        var handler = new CatalogCommandHandler(db, actor);
        var documentInput = new DocumentInput("Regelwerk", "Herausgeber", "1.0", null, null, DocumentUsageState.Active, null, null);

        var document = await handler.Handle(new CreateDocumentCommand(documentInput), CancellationToken.None);
        Assert.True(document.IsSuccess);
        var storedDocument = await db.Documents.FindAsync(document.Value);
        var update = await handler.Handle(new UpdateDocumentCommand(document.Value!, documentInput with { Title = "Aktualisiertes Regelwerk" }, storedDocument!.ConcurrencyVersion), CancellationToken.None);
        Assert.True(update.IsSuccess);

        var catalog = await handler.Handle(new CreateCatalogVersionCommand(document.Value!, null), CancellationToken.None);
        var invalidElement = await handler.Handle(new AddDocumentElementCommand(catalog.Value!, null, null, "Vorläufige Anforderung", null), CancellationToken.None);
        var invalidQuestion = await handler.Handle(new AddQuestionCommand(invalidElement.Value!, "Ist die vorläufige Anforderung erfüllt?", null, null, null, []), CancellationToken.None);
        var invalidReady = await handler.Handle(new SetCatalogReadyCommand(catalog.Value!, (await db.CatalogVersions.FindAsync(catalog.Value))!.ConcurrencyVersion, true), CancellationToken.None);
        Assert.False(invalidReady.IsSuccess);
        Assert.Equal("CATALOG.QUESTION_SCOPE_REQUIRED", Assert.Single(invalidReady.Errors).Code);
        Assert.True((await handler.Handle(new UpdateQuestionCommand(invalidQuestion.Value!, "Ist die vorläufige Anforderung erfüllt?", null, null, null, [1]), CancellationToken.None)).IsSuccess);

        var element = await handler.Handle(new AddDocumentElementCommand(catalog.Value!, null, "Kapitel", "Die Anforderung muss erfüllt sein.", null), CancellationToken.None);
        var question = await handler.Handle(new AddQuestionCommand(element.Value!, "Ist die Anforderung erfüllt?", null, null, null, [1]), CancellationToken.None);
        Assert.True(question.IsSuccess);
        Assert.True((await handler.Handle(new SetDocumentElementWeightCommand(element.Value!, 5), CancellationToken.None)).IsSuccess);
        Assert.True((await handler.Handle(new SetCatalogReadyCommand(catalog.Value!, (await db.CatalogVersions.FindAsync(catalog.Value))!.ConcurrencyVersion, true), CancellationToken.None)).IsSuccess);

        var immutable = await handler.Handle(new UpdateDocumentElementCommand(element.Value!, "Geändert", "Die Anforderung muss erfüllt sein.", null), CancellationToken.None);
        Assert.False(immutable.IsSuccess);
        Assert.Equal("CATALOG.NOT_DRAFT", Assert.Single(immutable.Errors).Code);

        var copy = await handler.Handle(new CopyCatalogVersionCommand(catalog.Value!, (await db.CatalogVersions.FindAsync(catalog.Value))!.ConcurrencyVersion), CancellationToken.None);
        Assert.True(copy.IsSuccess);
        var copiedElement = Assert.Single(db.DocumentElements.Where(x => x.CatalogVersionId == copy.Value && x.Text == "Die Anforderung muss erfüllt sein."));
        var copiedQuestion = Assert.Single(db.Questions.Where(x => x.ElementId == copiedElement.ElementId));
        Assert.NotEqual(element.Value, copiedElement.ElementId);
        Assert.NotEqual(question.Value, copiedQuestion.QuestionId);
        Assert.Equal(5, (await db.DocumentElementWeights.FindAsync(copiedElement.ElementId))!.Weight);
        Assert.Single(db.QuestionScopeTypes.Where(x => x.QuestionId == copiedQuestion.QuestionId));
    }

    [Fact]
    public async Task Delete_preview_counts_the_subtree_and_requires_confirmation()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var handler = new CatalogCommandHandler(db, new TestActor());
        var document = await handler.Handle(new CreateDocumentCommand(new("Regelwerk", null, null, null, null, DocumentUsageState.Active, null, null)), CancellationToken.None);
        var catalog = await handler.Handle(new CreateCatalogVersionCommand(document.Value!, null), CancellationToken.None);
        var root = await handler.Handle(new AddDocumentElementCommand(catalog.Value!, null, "Root", null, null), CancellationToken.None);
        var child = await handler.Handle(new AddDocumentElementCommand(catalog.Value!, root.Value, null, "Anforderung", null), CancellationToken.None);
        await handler.Handle(new AddQuestionCommand(child.Value!, "Ist die Anforderung erfüllt?", null, null, null, [1]), CancellationToken.None);

        var preview = await handler.Handle(new GetDeleteDocumentElementPreviewQuery(root.Value!), CancellationToken.None);
        Assert.Equal(new DeleteDocumentElementPreview(1, 1, 1), preview.Value);
        var rejected = await handler.Handle(new DeleteDocumentElementCommand(root.Value!, false), CancellationToken.None);
        Assert.Equal("CATALOG.DELETE_CONFIRMATION_REQUIRED", Assert.Single(rejected.Errors).Code);
        Assert.True((await handler.Handle(new DeleteDocumentElementCommand(root.Value!, true), CancellationToken.None)).IsSuccess);
        Assert.Empty(db.DocumentElements.Where(x => x.CatalogVersionId == catalog.Value));
    }

    private static async Task VerifyValidImportApplyAsync(string databaseProvider, string connectionString)
    {
        await using var provider = CreateProvider(databaseProvider, connectionString);
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var catalogHandler = new CatalogCommandHandler(db, new TestActor());
        var document = await catalogHandler.Handle(new CreateDocumentCommand(new("Regelwerk", null, null, null, null, DocumentUsageState.Active, null, null)), CancellationToken.None);
        var catalog = await catalogHandler.Handle(new CreateCatalogVersionCommand(document.Value!, null), CancellationToken.None);
        var handler = new CatalogImportHandler(db);

        var result = await handler.Handle(new ApplyCatalogImportCommand(ValidPackage(catalog.Value!, 1), ImportApplyMode.Full, []), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var root = Assert.Single(db.DocumentElements.Where(x => x.CatalogVersionId == catalog.Value));
        Assert.Single(db.Questions.Where(x => x.ElementId == root.ElementId));
        Assert.Equal(2, (await db.CatalogVersions.FindAsync(catalog.Value))!.DraftRevision);
    }

    private static ServiceProvider CreateProvider(string connectionString) => CreateProvider("PostgreSQL", connectionString);

    private static ServiceProvider CreateProvider(string databaseProvider, string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Auditarium:Database:Provider"] = databaseProvider, ["Auditarium:Database:ConnectionString"] = connectionString, ["Auditarium:Database:BootstrapTimeoutSeconds"] = "180" }).Build();
        var services = new ServiceCollection(); services.AddSingleton<IConfiguration>(configuration); services.AddAuditariumPersistence(configuration); return services.BuildServiceProvider();
    }

    private static string Package(long catalogVersionId, int draftRevision) => $$"""
        {
          "import_format_version": 1,
          "catalog_version_id": {{catalogVersionId}},
          "draft_revision": {{draftRevision}},
          "elements": [
            {
              "id": "valid-root", "parent_id": null, "sort_order": 0,
              "title": "Valid root", "text": "The requirement.", "notes": null, "weight": null,
              "questions": [
                { "sort_order": 0, "text": "Is the requirement fulfilled?", "verification_hint": null, "evidence_hint": null, "notes": null, "scope_keys": ["TECHNICAL_AREA"] }
              ]
            },
            {
              "id": "invalid-root", "parent_id": "missing-parent", "sort_order": 0,
              "title": "Invalid root", "text": "The invalid requirement.", "notes": null, "weight": null,
              "questions": []
            }
          ]
        }
        """;

    private static string ValidPackage(long catalogVersionId, int draftRevision) => $$"""
        {
          "import_format_version": 1,
          "catalog_version_id": {{catalogVersionId}},
          "draft_revision": {{draftRevision}},
          "elements": [
            {
              "id": "root", "parent_id": null, "sort_order": 0,
              "title": "Root", "text": "Requirement", "notes": null, "weight": null,
              "questions": [
                { "sort_order": 0, "text": "Is it fulfilled?", "verification_hint": null, "evidence_hint": null, "notes": null, "scope_keys": ["TECHNICAL_AREA"] }
              ]
            }
          ]
        }
        """;

    private sealed class TestActor : ICurrentActor { public ActorType Type => ActorType.System; public long? UserId => 0; public bool IsAuthenticated => true; }
}
