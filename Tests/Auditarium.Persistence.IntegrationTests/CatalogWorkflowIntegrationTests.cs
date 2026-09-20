// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Features.Catalog;
using Auditarium.Dal;
using Auditarium.Models.Catalog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

public sealed class CatalogWorkflowIntegrationTests
{
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
        var invalidReady = await handler.Handle(new SetCatalogReadyCommand(catalog.Value!, true), CancellationToken.None);
        Assert.False(invalidReady.IsSuccess);
        Assert.Equal("CATALOG.QUESTION_SCOPE_REQUIRED", Assert.Single(invalidReady.Errors).Code);
        Assert.True((await handler.Handle(new UpdateQuestionCommand(invalidQuestion.Value!, "Ist die vorläufige Anforderung erfüllt?", null, null, null, [1]), CancellationToken.None)).IsSuccess);

        var element = await handler.Handle(new AddDocumentElementCommand(catalog.Value!, null, "Kapitel", "Die Anforderung muss erfüllt sein.", null), CancellationToken.None);
        var question = await handler.Handle(new AddQuestionCommand(element.Value!, "Ist die Anforderung erfüllt?", null, null, null, [1]), CancellationToken.None);
        Assert.True(question.IsSuccess);
        Assert.True((await handler.Handle(new SetDocumentElementWeightCommand(element.Value!, 5), CancellationToken.None)).IsSuccess);
        Assert.True((await handler.Handle(new SetCatalogReadyCommand(catalog.Value!, true), CancellationToken.None)).IsSuccess);

        var immutable = await handler.Handle(new UpdateDocumentElementCommand(element.Value!, "Geändert", "Die Anforderung muss erfüllt sein.", null), CancellationToken.None);
        Assert.False(immutable.IsSuccess);
        Assert.Equal("CATALOG.NOT_DRAFT", Assert.Single(immutable.Errors).Code);

        var copy = await handler.Handle(new CopyCatalogVersionCommand(catalog.Value!), CancellationToken.None);
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

    private static ServiceProvider CreateProvider(string connectionString)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Auditarium:Database:Provider"] = "PostgreSQL", ["Auditarium:Database:ConnectionString"] = connectionString, ["Auditarium:Database:BootstrapTimeoutSeconds"] = "180" }).Build();
        var services = new ServiceCollection(); services.AddSingleton<IConfiguration>(configuration); services.AddAuditariumPersistence(configuration); return services.BuildServiceProvider();
    }
    private sealed class TestActor : ICurrentActor { public ActorType Type => ActorType.System; public long? UserId => 0; public bool IsAuthenticated => true; }
}
