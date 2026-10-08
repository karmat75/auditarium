// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Features.Audits;
using Auditarium.Bll.Features.Catalog;
using Auditarium.Dal;
using Auditarium.Models.Catalog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

public sealed class AuditWorkflowIntegrationTests
{
    [Fact]
    public async Task PostgreSql_empty_audit_and_audit_unit_lists_are_returned_successfully()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await VerifyEmptyListsAsync("PostgreSQL", container.GetConnectionString());
    }

    [Fact]
    public async Task SqlServer_empty_audit_and_audit_unit_lists_are_returned_successfully()
    {
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        await VerifyEmptyListsAsync("SqlServer", container.GetConnectionString());
    }

    [Fact]
    public async Task Audit_unit_list_sorting_returns_the_rows_in_the_requested_order()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var commands = new AuditCommandHandler(db, new TestActor());

        Assert.True((await commands.Handle(new CreateAuditUnitCommand(new(null, 6, "Beta", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None)).IsSuccess);
        Assert.True((await commands.Handle(new CreateAuditUnitCommand(new(null, 2, "Alpha", null, AuditUnitUsageState.Inactive, "Nicht verwendet", null)), CancellationToken.None)).IsSuccess);
        Assert.True((await commands.Handle(new CreateAuditUnitCommand(new(null, 1, "Gamma", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None)).IsSuccess);

        var queries = new AuditQueryHandler(db);
        var byName = await queries.Handle(new ListAuditUnitsQuery(null, 0, 25, "name", false), CancellationToken.None);
        var byScopeType = await queries.Handle(new ListAuditUnitsQuery(null, 0, 25, "scopeType", false), CancellationToken.None);
        var byUsageState = await queries.Handle(new ListAuditUnitsQuery(null, 0, 25, "usageState", true), CancellationToken.None);

        Assert.Equal(["Alpha", "Beta", "Gamma"], byName.Value!.Items.Select(item => item.Name));
        Assert.Equal(["Gamma", "Alpha", "Beta"], byScopeType.Value!.Items.Select(item => item.Name));
        Assert.Equal("Alpha", byUsageState.Value!.Items[0].Name);
    }

    [Fact]
    public async Task Audit_unit_hierarchy_keeps_parent_paths_when_searching_and_sorts_only_siblings()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var commands = new AuditCommandHandler(db, new TestActor());
        var root = await commands.Handle(new CreateAuditUnitCommand(new(null, 1, "Root", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        var zulu = await commands.Handle(new CreateAuditUnitCommand(new(root.Value, 2, "Zulu", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        var alpha = await commands.Handle(new CreateAuditUnitCommand(new(root.Value, 2, "Alpha", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        var leaf = await commands.Handle(new CreateAuditUnitCommand(new(alpha.Value, 3, "Needle", null, AuditUnitUsageState.Inactive, "Nicht verwendet", null)), CancellationToken.None);
        Assert.True(root.IsSuccess && zulu.IsSuccess && alpha.IsSuccess && leaf.IsSuccess);

        var queries = new AuditQueryHandler(db);
        var sorted = await queries.Handle(new GetAuditUnitHierarchyQuery(null, "name", false), CancellationToken.None);
        var searched = await queries.Handle(new GetAuditUnitHierarchyQuery("Needle"), CancellationToken.None);
        var searchedItems = searched.Value!;

        Assert.Equal(["Root", "Alpha", "Needle", "Zulu"], sorted.Value!.Select(x => x.Name));
        Assert.Equal(["Root", "Alpha", "Needle"], searchedItems.Select(x => x.Name));
        Assert.Equal([0, 1, 2], searchedItems.Select(x => x.Depth));
        Assert.All(searchedItems, item => Assert.False(string.IsNullOrWhiteSpace(item.ScopeTypeKey)));
        Assert.Equal(root.Value, searchedItems[1].ParentAuditUnitId);
    }

    private static async Task VerifyEmptyListsAsync(string databaseProvider, string connectionString)
    {
        await using var provider = CreateProvider(databaseProvider, connectionString);
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var queries = new AuditQueryHandler(db);

        var auditUnits = await queries.Handle(new ListAuditUnitsQuery(null, 0, 200, "name", false), CancellationToken.None);
        var hierarchy = await queries.Handle(new GetAuditUnitHierarchyQuery(), CancellationToken.None);
        var audits = await queries.Handle(new ListAuditsQuery(null, null, 0, 200, "createdAt", true), CancellationToken.None);

        Assert.True(auditUnits.IsSuccess);
        Assert.Empty(auditUnits.Value!.Items);
        Assert.True(hierarchy.IsSuccess);
        Assert.Empty(hierarchy.Value!);
        Assert.True(audits.IsSuccess);
        Assert.Empty(audits.Value!.Items);
    }

    [Fact]
    public async Task Audit_is_materialized_claimed_answered_finalized_and_keeps_its_catalog_version_immutable()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var actor = new TestActor();
        var catalog = new CatalogCommandHandler(db, actor);
        var document = await catalog.Handle(new CreateDocumentCommand(new("Regelwerk", null, null, null, null, DocumentUsageState.Active, null, null)), CancellationToken.None);
        var version = await catalog.Handle(new CreateCatalogVersionCommand(document.Value!, null), CancellationToken.None);
        var element = await catalog.Handle(new AddDocumentElementCommand(version.Value!, null, null, "Anforderung", null), CancellationToken.None);
        Assert.True((await catalog.Handle(new AddQuestionCommand(element.Value!, "Erfüllt?", null, null, null, [6]), CancellationToken.None)).IsSuccess);
        Assert.True((await catalog.Handle(new SetCatalogReadyCommand(version.Value!, (await db.CatalogVersions.FindAsync(version.Value))!.ConcurrencyVersion, true), CancellationToken.None)).IsSuccess);

        var audits = new AuditCommandHandler(db, actor);
        var unit = await audits.Handle(new CreateAuditUnitCommand(new(null, 6, "Serverraum", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        var audit = await audits.Handle(new CreateAuditCommand(new("Audit", null, unit.Value!, version.Value!, null, null)), CancellationToken.None);
        var preview = await audits.Handle(new GetAuditPreviewQuery(audit.Value!), CancellationToken.None);
        Assert.Equal(new AuditPreview(1, 1), preview.Value);
        Assert.True((await audits.Handle(new PublishAuditCommand(audit.Value!), CancellationToken.None)).IsSuccess);
        var stored = await db.Audits.FindAsync(audit.Value);
        Assert.Equal(AuditState.Ready, stored!.AuditState);
        Assert.NotNull(stored.AuditUnitContext);
        Assert.Single(db.AuditDocumentElements.Where(x => x.AuditId == audit.Value));
        var question = Assert.Single(db.AuditQuestions);

        Assert.True((await audits.Handle(new ClaimAuditCommand(audit.Value!, stored.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        var missingComment = await audits.Handle(new AnswerAuditQuestionCommand(audit.Value!, question.AuditQuestionId, AuditQuestionResult.NotApplicable, null, null, question.ConcurrencyVersion), CancellationToken.None);
        Assert.Equal("AUDIT_QUESTION.RESPONSE_POLICY_VIOLATION", Assert.Single(missingComment.Errors).Code);
        Assert.True((await audits.Handle(new AnswerAuditQuestionCommand(audit.Value!, question.AuditQuestionId, AuditQuestionResult.NotApplicable, "Nicht installiert", null, question.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        stored = await db.Audits.FindAsync(audit.Value);
        Assert.Equal(AuditState.InProgress, stored!.AuditState);
        question = (await db.AuditQuestions.FindAsync(question.AuditQuestionId))!;
        Assert.True((await audits.Handle(new ResetAuditQuestionCommand(audit.Value!, question.AuditQuestionId, question.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        stored = await db.Audits.FindAsync(audit.Value);
        Assert.Equal(AuditState.Ready, stored!.AuditState);
        Assert.True((await audits.Handle(new CancelAuditCommand(audit.Value!, "Termin entfällt", stored.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        stored = await db.Audits.FindAsync(audit.Value);
        Assert.Equal(AuditState.Canceled, stored!.AuditState);
        Assert.Null(stored.AssignedAuditorUserId);
        Assert.True((await audits.Handle(new ReopenAuditCommand(audit.Value!, "Termin ist wieder möglich", stored.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        stored = await db.Audits.FindAsync(audit.Value);
        Assert.Equal(AuditState.Ready, stored!.AuditState);
        Assert.True((await audits.Handle(new ClaimAuditCommand(audit.Value!, stored.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        question = (await db.AuditQuestions.FindAsync(question.AuditQuestionId))!;
        Assert.True((await audits.Handle(new AnswerAuditQuestionCommand(audit.Value!, question.AuditQuestionId, AuditQuestionResult.NotApplicable, "Nicht installiert", null, question.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        question = (await db.AuditQuestions.FindAsync(question.AuditQuestionId))!;
        question.Comment = null;
        await db.SaveChangesAsync();
        stored = await db.Audits.FindAsync(audit.Value);
        var finalizationBlocked = await audits.Handle(new FinalizeAuditCommand(audit.Value!, stored!.ConcurrencyVersion), CancellationToken.None);
        Assert.Equal("AUDIT_QUESTION.RESPONSE_POLICY_VIOLATION", Assert.Single(finalizationBlocked.Errors).Code);
        question.Comment = "Nicht installiert";
        await db.SaveChangesAsync();
        stored = await db.Audits.FindAsync(audit.Value);
        Assert.True((await audits.Handle(new FinalizeAuditCommand(audit.Value!, stored!.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        Assert.Equal(AuditState.Finalized, (await db.Audits.FindAsync(audit.Value))!.AuditState);
        Assert.Null((await db.Audits.FindAsync(audit.Value))!.AssignedAuditorUserId);

        var blocked = await catalog.Handle(new SetCatalogDraftCommand(version.Value!, (await db.CatalogVersions.FindAsync(version.Value))!.ConcurrencyVersion, true), CancellationToken.None);
        Assert.Equal("CATALOG.USED_VERSION_CREATE_DRAFT_COPY", Assert.Single(blocked.Errors).Code);
    }

    [Fact]
    public async Task Audit_unit_parent_scope_and_response_reset_rules_are_enforced()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var handler = new AuditCommandHandler(db, new TestActor());
        var site = await handler.Handle(new CreateAuditUnitCommand(new(null, 2, "Standort", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        var invalid = await handler.Handle(new CreateAuditUnitCommand(new(site.Value, 9, "Anwendung", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        Assert.Equal("AUDIT_UNIT.PARENT_SCOPE_INVALID", Assert.Single(invalid.Errors).Code);
        var other = await handler.Handle(new CreateAuditUnitCommand(new(null, 13, "Sonderfall", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        Assert.Equal("AUDIT_UNIT.OTHER_DESCRIPTION_REQUIRED", Assert.Single(other.Errors).Code);
    }

    [Fact]
    public async Task Draft_preview_does_not_materialize_and_invalid_publish_leaves_the_draft_unchanged()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var catalog = new CatalogCommandHandler(db, new TestActor());
        var document = await catalog.Handle(new CreateDocumentCommand(new("Regelwerk", null, null, null, null, DocumentUsageState.Active, null, null)), CancellationToken.None);
        var version = await catalog.Handle(new CreateCatalogVersionCommand(document.Value!, null), CancellationToken.None);
        var element = await catalog.Handle(new AddDocumentElementCommand(version.Value!, null, null, "Anforderung", null), CancellationToken.None);
        Assert.True((await catalog.Handle(new AddQuestionCommand(element.Value!, "Erfüllt?", null, null, null, [6]), CancellationToken.None)).IsSuccess);

        var audits = new AuditCommandHandler(db, new TestActor());
        var unit = await audits.Handle(new CreateAuditUnitCommand(new(null, 6, "Serverraum", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        var audit = await audits.Handle(new CreateAuditCommand(new("Audit", null, unit.Value!, version.Value!, null, null)), CancellationToken.None);

        Assert.Equal(new AuditPreview(1, 1), (await audits.Handle(new GetAuditPreviewQuery(audit.Value!), CancellationToken.None)).Value);
        Assert.Empty(db.AuditDocumentElements);
        Assert.Empty(db.AuditQuestions);

        var publish = await audits.Handle(new PublishAuditCommand(audit.Value!), CancellationToken.None);
        Assert.Equal("AUDIT.CATALOG_NOT_USABLE", Assert.Single(publish.Errors).Code);
        Assert.Empty(db.AuditDocumentElements);
        Assert.Empty(db.AuditQuestions);
        Assert.Equal(AuditState.Draft, (await db.Audits.FindAsync(audit.Value))!.AuditState);
    }

    [Fact]
    public async Task Audit_data_queries_share_explicit_filters_context_and_historical_snapshots()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var actor = new TestActor();
        var catalog = new CatalogCommandHandler(db, actor);
        var document = await catalog.Handle(new CreateDocumentCommand(new("Regelwerk v1", null, "1.0", null, null, DocumentUsageState.Active, null, null)), CancellationToken.None);
        var version = await catalog.Handle(new CreateCatalogVersionCommand(document.Value!, null), CancellationToken.None);
        var element = await catalog.Handle(new AddDocumentElementCommand(version.Value!, null, null, "Anforderung", null), CancellationToken.None);
        await catalog.Handle(new AddQuestionCommand(element.Value!, "Erfüllt?", "Prüfen", "Nachweis", null, [6]), CancellationToken.None);
        Assert.True((await catalog.Handle(new SetCatalogReadyCommand(version.Value!, (await db.CatalogVersions.FindAsync(version.Value))!.ConcurrencyVersion, true), CancellationToken.None)).IsSuccess);

        var commands = new AuditCommandHandler(db, actor);
        var unit = await commands.Handle(new CreateAuditUnitCommand(new(null, 6, "Serverraum", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        var audit = await commands.Handle(new CreateAuditCommand(new("Audit", null, unit.Value!, version.Value!, null, null)), CancellationToken.None);
        Assert.True((await commands.Handle(new PublishAuditCommand(audit.Value!), CancellationToken.None)).IsSuccess);
        var storedQuestion = Assert.Single(db.AuditQuestions);
        storedQuestion.Result = AuditQuestionResult.No;
        storedQuestion.Comment = "Abweichung";
        storedQuestion.AnsweredAt = DateTimeOffset.UtcNow;
        storedQuestion.AnsweredBy = 0;
        var storedDocument = (await db.Documents.FindAsync(document.Value))!;
        storedDocument.Title = "Regelwerk umbenannt";
        storedDocument.Version = "1.1";
        var storedUnit = (await db.AuditUnits.FindAsync(unit.Value))!;
        storedUnit.Name = "Serverraum umbenannt";
        await db.SaveChangesAsync();

        var queries = new AuditDataQueryHandler(db);
        var filter = new AuditDataFilter(ScopeTypeId: 6, AuditUnitId: unit.Value, DocumentId: document.Value, CatalogVersionId: version.Value, AuditState: AuditState.Ready);
        var audits = await queries.Handle(new ListAuditDataAuditsQuery(filter, 0, 50, AuditDataAuditSort.CreatedAt, true), CancellationToken.None);
        var elements = await queries.Handle(new ListAuditDataElementsQuery(filter, 0, 50, AuditDataElementSort.ElementTitle, false), CancellationToken.None);
        var questions = await queries.Handle(new ListAuditDataQuestionsQuery(filter, 0, 50, AuditDataQuestionSort.QuestionText, false), CancellationToken.None);

        Assert.True(audits.IsSuccess);
        Assert.Equal("Serverraum", Assert.Single(audits.Value!.Items).Audit.AuditUnitName);
        Assert.Equal("Regelwerk umbenannt", audits.Value.Items[0].Audit.DocumentTitle);
        Assert.Equal("1.1", audits.Value.Items[0].Audit.DocumentVersion);
        Assert.Equal(version.Value, audits.Value.Items[0].Audit.CatalogVersionId);
        Assert.True(elements.IsSuccess);
        Assert.Equal(AuditDocumentElementResult.NotFulfilled, Assert.Single(elements.Value!.Items).Result);
        Assert.True(questions.IsSuccess);
        Assert.Equal(storedQuestion.AuditQuestionId, Assert.Single(questions.Value!.Items).AuditQuestionId);
        Assert.Equal("Abweichung", questions.Value.Items[0].Comment);

        var invalid = await queries.Handle(new ListAuditDataAuditsQuery(new AuditDataFilter(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(-1)), 0, 50, AuditDataAuditSort.CreatedAt, true), CancellationToken.None);
        Assert.False(invalid.IsSuccess);
        Assert.Equal("AUDIT_DATA.QUERY_ARGUMENT_INVALID", Assert.Single(invalid.Errors).Code);
    }

    [Fact]
    public async Task Csv_export_uses_shared_data_selection_escapes_values_and_requires_audit_logging()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var provider = CreateProvider(container.GetConnectionString());
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var actor = new TestActor();
        var catalog = new CatalogCommandHandler(db, actor);
        var document = await catalog.Handle(new CreateDocumentCommand(new("Regelwerk, \"Export\"", null, "1.0", null, null, DocumentUsageState.Active, null, null)), CancellationToken.None);
        var version = await catalog.Handle(new CreateCatalogVersionCommand(document.Value!, null), CancellationToken.None);
        var element = await catalog.Handle(new AddDocumentElementCommand(version.Value!, null, null, "Element", null), CancellationToken.None);
        await catalog.Handle(new AddQuestionCommand(element.Value!, "Frage", null, null, null, [6]), CancellationToken.None);
        Assert.True((await catalog.Handle(new SetCatalogReadyCommand(version.Value!, (await db.CatalogVersions.FindAsync(version.Value))!.ConcurrencyVersion, true), CancellationToken.None)).IsSuccess);
        var commands = new AuditCommandHandler(db, actor);
        var unit = await commands.Handle(new CreateAuditUnitCommand(new(null, 6, "Serverraum", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        var audit = await commands.Handle(new CreateAuditCommand(new("Audit", null, unit.Value!, version.Value!, null, null)), CancellationToken.None);
        Assert.True((await commands.Handle(new PublishAuditCommand(audit.Value!), CancellationToken.None)).IsSuccess);
        var storedQuestion = Assert.Single(db.AuditQuestions);
        storedQuestion.Result = AuditQuestionResult.No;
        storedQuestion.Comment = "Zeile 1\nZeile, \"2\"";
        storedQuestion.AnsweredAt = DateTimeOffset.UtcNow;
        storedQuestion.AnsweredBy = 0;
        await db.SaveChangesAsync();

        var events = new RecordingAuditEvents();
        var export = new AuditDataQueryHandler(db, events);
        foreach (var level in Enum.GetValues<AuditCsvExportLevel>())
        {
            var result = await export.Handle(new ExportAuditDataCsvQuery(level, new AuditDataFilter(DocumentId: document.Value)), CancellationToken.None);
            Assert.True(result.IsSuccess);
            var csv = System.Text.Encoding.UTF8.GetString(result.Value!.Content);
            Assert.StartsWith("\uFEFF", csv);
            Assert.Contains("\"Regelwerk, \"\"Export\"\"\"", csv);
            if (level == AuditCsvExportLevel.Questions) Assert.Contains("\"Zeile 1\nZeile, \"\"2\"\"\"", csv);
        }
        Assert.Equal(3, events.Events.Count);
        Assert.All(events.Events, auditEvent => Assert.Equal("EXPORTED", auditEvent.Action));

        var failed = new AuditDataQueryHandler(db, new FailingAuditEvents());
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await failed.Handle(new ExportAuditDataCsvQuery(AuditCsvExportLevel.Audits, new()), CancellationToken.None));
    }

    private static ServiceProvider CreateProvider(string connectionString) => CreateProvider("PostgreSQL", connectionString);

    private static ServiceProvider CreateProvider(string databaseProvider, string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Auditarium:Database:Provider"] = databaseProvider, ["Auditarium:Database:ConnectionString"] = connectionString, ["Auditarium:Database:BootstrapTimeoutSeconds"] = "180" }).Build();
        var services = new ServiceCollection(); services.AddSingleton<IConfiguration>(configuration); services.AddAuditariumPersistence(configuration); return services.BuildServiceProvider();
    }

    private sealed class TestActor : ICurrentActor { public ActorType Type => ActorType.System; public long? UserId => 0; public bool IsAuthenticated => true; }
    private sealed class RecordingAuditEvents : IAuditEventWriter
    {
        public List<AuditEvent> Events { get; } = [];
        public Task WriteAsync(AuditEvent eventData, CancellationToken cancellationToken = default) { Events.Add(eventData); return Task.CompletedTask; }
    }
    private sealed class FailingAuditEvents : IAuditEventWriter
    {
        public Task WriteAsync(AuditEvent eventData, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Audit log unavailable");
    }
}
