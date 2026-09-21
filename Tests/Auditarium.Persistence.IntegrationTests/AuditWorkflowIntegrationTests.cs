// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Features.Audits;
using Auditarium.Bll.Features.Catalog;
using Auditarium.Dal;
using Auditarium.Models.Catalog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

public sealed class AuditWorkflowIntegrationTests
{
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

    private static ServiceProvider CreateProvider(string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Auditarium:Database:Provider"] = "PostgreSQL", ["Auditarium:Database:ConnectionString"] = connectionString, ["Auditarium:Database:BootstrapTimeoutSeconds"] = "180" }).Build();
        var services = new ServiceCollection(); services.AddSingleton<IConfiguration>(configuration); services.AddAuditariumPersistence(configuration); return services.BuildServiceProvider();
    }

    private sealed class TestActor : ICurrentActor { public ActorType Type => ActorType.System; public long? UserId => 0; public bool IsAuthenticated => true; }
}
