// SPDX-License-Identifier: MIT
using Auditarium.Api;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Features.Audits;
using Auditarium.Bll.Features.Catalog;
using Auditarium.Dal;
using Auditarium.Models.Catalog;
using Auditarium.Web.Pages.Analysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

/// <summary>Normative 20.10.4 provider matrix and server-room reference scenario.</summary>
public sealed class ProviderMatrixReferenceScenarioIntegrationTests
{
    [Fact]
    public async Task PostgreSql_runs_the_reference_scenario_from_an_empty_database()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await RunReferenceScenarioAsync("PostgreSQL", container.GetConnectionString());
    }

    [Fact]
    public async Task SqlServer_runs_the_reference_scenario_from_an_empty_database()
    {
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();
        await RunReferenceScenarioAsync("SqlServer", container.GetConnectionString());
    }

    private static async Task RunReferenceScenarioAsync(string databaseProvider, string connectionString)
    {
        await using var provider = CreateProvider(databaseProvider, connectionString);
        await provider.InitializeAuditariumDatabaseAsync();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        var actor = new TestActor();
        var audits = new AuditCommandHandler(db, actor);
        var catalog = new CatalogCommandHandler(db, actor);

        var technicalArea = await db.ScopeTypes.SingleAsync(x => x.Key == "TECHNICAL_AREA");
        var organization = await audits.Handle(new CreateAuditUnitCommand(new(null, 1, "Musterorganisation", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        var site = await audits.Handle(new CreateAuditUnitCommand(new(organization.Value, 2, "Standort Musterstadt", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        var building = await audits.Handle(new CreateAuditUnitCommand(new(site.Value, 3, "Gebäude A", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        var serverRoom = await audits.Handle(new CreateAuditUnitCommand(new(building.Value, technicalArea.ScopeTypeId, "Serverraum R-01", null, AuditUnitUsageState.Active, null, null)), CancellationToken.None);
        Assert.All([organization, site, building, serverRoom], result => Assert.True(result.IsSuccess));

        var document = await catalog.Handle(new CreateDocumentCommand(new("Betriebsregeln für technische Räume – Referenzregelwerk", null, "1.0", null, null, DocumentUsageState.Active, null, null)), CancellationToken.None);
        var catalogVersion = await catalog.Handle(new CreateCatalogVersionCommand(document.Value!, null), CancellationToken.None);
        Assert.True(document.IsSuccess);
        Assert.True(catalogVersion.IsSuccess);

        foreach (var group in Requirements)
        {
            var root = await catalog.Handle(new AddDocumentElementCommand(catalogVersion.Value!, null, group.Topic, null, null), CancellationToken.None);
            Assert.True(root.IsSuccess);
            foreach (var requirement in group.Requirements)
            {
                var element = await catalog.Handle(new AddDocumentElementCommand(catalogVersion.Value!, root.Value, requirement.Reference, requirement.Text, null), CancellationToken.None);
                Assert.True(element.IsSuccess);
                Assert.True((await catalog.Handle(new AddQuestionCommand(element.Value!, requirement.FirstQuestion, null, null, null, [technicalArea.ScopeTypeId]), CancellationToken.None)).IsSuccess);
                Assert.True((await catalog.Handle(new AddQuestionCommand(element.Value!, requirement.SecondQuestion, null, null, null, [technicalArea.ScopeTypeId]), CancellationToken.None)).IsSuccess);
                Assert.True((await catalog.Handle(new SetDocumentElementWeightCommand(element.Value!, 3), CancellationToken.None)).IsSuccess);
            }
        }

        var ready = await catalog.Handle(new SetCatalogReadyCommand(catalogVersion.Value!, (await db.CatalogVersions.FindAsync(catalogVersion.Value))!.ConcurrencyVersion, true), CancellationToken.None);
        Assert.True(ready.IsSuccess);
        Assert.Equal(CatalogState.Ready, (await db.CatalogVersions.FindAsync(catalogVersion.Value))!.CatalogState);

        var settings = new AuditSettings(new Dictionary<AuditQuestionResult, ResponseRule>
        {
            [AuditQuestionResult.Yes] = new(false, false),
            [AuditQuestionResult.No] = new(true, false),
            [AuditQuestionResult.NotApplicable] = new(true, false),
            [AuditQuestionResult.NotDeterminable] = new(true, false)
        });
        var input = new AuditInput("Serverraum R-01 – Erstprüfung", null, serverRoom.Value!, catalogVersion.Value!, settings, null);
        var firstAudit = await audits.Handle(new CreateAuditCommand(input), CancellationToken.None);
        Assert.True(firstAudit.IsSuccess);
        Assert.Equal(new AuditPreview(10, 20), (await audits.Handle(new GetAuditPreviewQuery(firstAudit.Value!), CancellationToken.None)).Value);
        Assert.True((await audits.Handle(new PublishAuditCommand(firstAudit.Value!), CancellationToken.None)).IsSuccess);
        Assert.Equal(AuditState.Ready, (await db.Audits.FindAsync(firstAudit.Value))!.AuditState);
        Assert.Equal(10, await db.AuditDocumentElements.CountAsync(x => x.AuditId == firstAudit.Value));
        Assert.Equal(20, await QuestionsForAudit(db, firstAudit.Value!).CountAsync());
        Assert.All(await QuestionsForAudit(db, firstAudit.Value!).ToListAsync(), question => Assert.Null(question.Result));

        await AnswerAndFinalizeAsync(audits, db, firstAudit.Value!, FirstAnswers);
        var firstResults = await ElementResultsAsync(db, firstAudit.Value!);
        AssertResults(firstResults, ["R02", "R04", "R05", "R08", "R09"], ["R01", "R03", "R07"], ["R06"], ["R10"]);

        var repeatInput = input with { Name = "Serverraum R-01 – Wiederholung" };
        var repeatedAudit = await audits.Handle(new CreateAuditRepeatCommand(firstAudit.Value!, repeatInput), CancellationToken.None);
        Assert.True(repeatedAudit.IsSuccess);
        Assert.True((await audits.Handle(new PublishAuditCommand(repeatedAudit.Value!), CancellationToken.None)).IsSuccess);
        var repeated = await db.Audits.FindAsync(repeatedAudit.Value);
        Assert.Equal(firstAudit.Value, repeated!.OriginAuditId);
        Assert.All(await QuestionsForAudit(db, repeatedAudit.Value!).ToListAsync(), question => { Assert.Null(question.Result); Assert.Null(question.Comment); Assert.Null(question.Evidence); });

        await AnswerAndFinalizeAsync(audits, db, repeatedAudit.Value!, RepeatAnswers);
        var repeatedResults = await ElementResultsAsync(db, repeatedAudit.Value!);
        AssertResults(repeatedResults, ["R01", "R02", "R03", "R04", "R05", "R06", "R07", "R08", "R09"], [], [], ["R10"]);
        Assert.Equal(firstResults, await ElementResultsAsync(db, firstAudit.Value!));
        Assert.All(await db.Audits.Where(x => x.AuditId == firstAudit.Value || x.AuditId == repeatedAudit.Value).ToListAsync(), audit => Assert.Equal(AuditState.Finalized, audit.AuditState));

        var filter = new AuditDataFilter(AuditUnitId: serverRoom.Value, CatalogVersionId: catalogVersion.Value, AuditState: AuditState.Finalized);
        var access = new AuditDataQueryHandler(db, new RecordingAuditEvents());
        var uiFilter = new AnalysisFilterInputModel { AuditUnitId = serverRoom.Value, CatalogVersionId = catalogVersion.Value, AuditState = AuditState.Finalized }.ToBllFilter();
        var uiRows = await access.Handle(new ListAuditDataAuditsQuery(uiFilter, 0, 50, AuditDataAuditSort.CreatedAt, false), CancellationToken.None);
        Assert.True(uiRows.IsSuccess);
        Assert.Equal(2, uiRows.Value!.TotalCount);

        var apiFilter = new AuditDataFilterContract(AuditUnitId: serverRoom.Value, CatalogVersionId: catalogVersion.Value, AuditState: AuditState.Finalized).ToBll();
        Assert.Equal(filter, apiFilter);
        var apiRows = await access.Handle(new ListAuditDataQuestionsQuery(apiFilter, 0, 50, AuditDataQuestionSort.QuestionText, false), CancellationToken.None);
        Assert.True(apiRows.IsSuccess);
        Assert.Equal(40, apiRows.Value!.TotalCount);
        Assert.Equal(40, apiRows.Value.Items.Select(AuditDataQuestionContract.From).Count());

        var csv = await access.Handle(new ExportAuditDataCsvQuery(AuditCsvExportLevel.Questions, filter), CancellationToken.None);
        Assert.True(csv.IsSuccess);
        var csvText = System.Text.Encoding.UTF8.GetString(csv.Value!.Content);
        Assert.Contains(firstAudit.Value!.ToString(System.Globalization.CultureInfo.InvariantCulture), csvText, StringComparison.Ordinal);
        Assert.Contains(repeatedAudit.Value!.ToString(System.Globalization.CultureInfo.InvariantCulture), csvText, StringComparison.Ordinal);
        Assert.Equal(41, csvText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Length);
    }

    private static async Task AnswerAndFinalizeAsync(AuditCommandHandler handler, AuditariumDbContext db, long auditId, IReadOnlyDictionary<string, Answer> answers)
    {
        var audit = (await db.Audits.FindAsync(auditId))!;
        Assert.True((await handler.Handle(new ClaimAuditCommand(auditId, audit.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        var questions = await (from question in db.AuditQuestions
                               join element in db.AuditDocumentElements on question.AuditDocumentElementId equals element.AuditDocumentElementId
                               join definition in db.DocumentElements on element.ElementId equals definition.ElementId
                               join questionDefinition in db.Questions on question.QuestionId equals questionDefinition.QuestionId
                               where element.AuditId == auditId
                               select new { question, Reference = definition.Title!, questionDefinition.SortOrder }).ToListAsync();
        foreach (var question in questions)
        {
            var answer = answers[$"{question.Reference}.{question.SortOrder + 1}"];
            Assert.True((await handler.Handle(new AnswerAuditQuestionCommand(auditId, question.question.AuditQuestionId, answer.Result, answer.Comment, null, question.question.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
        }

        audit = (await db.Audits.FindAsync(auditId))!;
        Assert.True((await handler.Handle(new FinalizeAuditCommand(auditId, audit.ConcurrencyVersion), CancellationToken.None)).IsSuccess);
    }

    private static async Task<Dictionary<string, AuditDocumentElementResult?>> ElementResultsAsync(AuditariumDbContext db, long auditId)
    {
        var rows = await new AuditDataQueryHandler(db).Handle(new ListAuditDataElementsQuery(new AuditDataFilter(), 0, 200, AuditDataElementSort.ElementTitle, false), CancellationToken.None);
        Assert.True(rows.IsSuccess);
        return rows.Value!.Items.Where(x => x.Audit.AuditId == auditId).ToDictionary(x => x.ElementTitle!, x => x.Result);
    }

    private static void AssertResults(IReadOnlyDictionary<string, AuditDocumentElementResult?> results, IEnumerable<string> fulfilled, IEnumerable<string> notFulfilled, IEnumerable<string> notDeterminable, IEnumerable<string> notApplicable)
    {
        Assert.Equal(10, results.Count);
        Assert.All(fulfilled, reference => Assert.Equal(AuditDocumentElementResult.Fulfilled, results[reference]));
        Assert.All(notFulfilled, reference => Assert.Equal(AuditDocumentElementResult.NotFulfilled, results[reference]));
        Assert.All(notDeterminable, reference => Assert.Equal(AuditDocumentElementResult.NotDeterminable, results[reference]));
        Assert.All(notApplicable, reference => Assert.Equal(AuditDocumentElementResult.NotApplicable, results[reference]));
    }

    private static IQueryable<AuditQuestion> QuestionsForAudit(AuditariumDbContext db, long auditId) => db.AuditQuestions.Where(question => db.AuditDocumentElements.Where(element => element.AuditId == auditId).Select(element => element.AuditDocumentElementId).Contains(question.AuditDocumentElementId));

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

    private sealed class TestActor : ICurrentActor { public ActorType Type => ActorType.System; public long? UserId => 0; public bool IsAuthenticated => true; }
    private sealed class RecordingAuditEvents : IAuditEventWriter { public Task WriteAsync(AuditEvent eventData, CancellationToken cancellationToken = default) => Task.CompletedTask; }
    private sealed record Answer(AuditQuestionResult Result, string? Comment = null);
    private sealed record Requirement(string Reference, string Text, string FirstQuestion, string SecondQuestion);
    private sealed record RequirementGroup(string Topic, IReadOnlyList<Requirement> Requirements);

    private static readonly IReadOnlyList<RequirementGroup> Requirements =
    [
        new("Zuständigkeit und Dokumentation", [
            new("R01", "Für den Raum müssen eine verantwortliche Person und eine Vertretung benannt sein.", "Ist eine verantwortliche Person für den Raum benannt?", "Ist eine Vertretung für die verantwortliche Person benannt?"),
            new("R02", "Der Raum muss durch eine aktuelle Beschreibung seiner Nutzung und einen aktuellen Aufstellungsplan dokumentiert sein.", "Beschreibt die Raumdokumentation die aktuelle Nutzung des Raums?", "Entspricht der Aufstellungsplan der aktuellen Aufstellung der Racks?")]),
        new("Zutritt und Schlüssel", [
            new("R03", "Die Zutrittsberechtigten müssen vollständig erfasst und ihre Berechtigungen durch die verantwortliche Person freigegeben sein.", "Sind alle zum Raum zutrittsberechtigten Personen in der Berechtigtenliste erfasst?", "Sind alle aufgeführten Zutrittsberechtigungen durch die verantwortliche Person freigegeben?"),
            new("R04", "Ausgegebene Raumschlüssel müssen ihren empfangenden Personen zugeordnet und die Rückgabe bei Wegfall der Berechtigung geregelt sein.", "Ist jeder ausgegebene Raumschlüssel einer empfangenden Person zugeordnet?", "Ist die Rückgabe des Raumschlüssels bei Wegfall der Berechtigung geregelt?")]),
        new("Fremdzutritt und Arbeiten", [
            new("R05", "Externe Besucher müssen im Raum begleitet werden; ihre Besuche müssen dokumentiert sein.", "Wurden externe Besucher bei den dokumentierten Besuchen im Raum begleitet?", "Sind die im Prüfzeitraum erfolgten externen Besuche dokumentiert?"),
            new("R06", "Wartungsarbeiten im Raum müssen vor Beginn freigegeben und ihr ordnungsgemäßer Abschluss dokumentiert werden.", "Wurden die Wartungsarbeiten im Prüfzeitraum jeweils vor Beginn freigegeben?", "Ist der ordnungsgemäße Abschluss der Wartungsarbeiten im Prüfzeitraum dokumentiert?")]),
        new("Raumzustand", [
            new("R07", "Die im Aufstellungsplan bezeichneten Zugänge zu den Racks müssen frei und vor Ort erkennbar markiert sein.", "Sind die im Aufstellungsplan bezeichneten Zugänge zu den Racks frei?", "Sind die im Aufstellungsplan bezeichneten Zugänge vor Ort erkennbar markiert?"),
            new("R08", "Die Racktüren müssen abschließbar sein; im Raum dürfen keine Gegenstände ohne vorgesehenen betrieblichen Zweck gelagert werden.", "Sind die Türen beider Racks abschließbar?", "Werden im Raum ausschließlich Gegenstände mit vorgesehenem betrieblichen Zweck aufbewahrt?")]),
        new("Kontrollen und Ausstattung", [
            new("R09", "Die Raumtemperatur muss ablesbar und der für diesen Raum festgelegte zulässige Temperaturbereich dokumentiert sein.", "Ist die aktuelle Raumtemperatur am vorhandenen Sensor ablesbar?", "Ist der für diesen Raum festgelegte zulässige Temperaturbereich dokumentiert?"),
            new("R10", "Falls Sensorik für Wassereintritt installiert ist, müssen eine dokumentierte Funktionsprüfung und ein dokumentierter Alarmierungsweg vorhanden sein.", "Liegt für die installierte Sensorik für Wassereintritt eine dokumentierte Funktionsprüfung vor?", "Ist für die installierte Sensorik für Wassereintritt der Alarmierungsweg dokumentiert?")])
    ];

    private static readonly IReadOnlyDictionary<string, Answer> FirstAnswers = new Dictionary<string, Answer>
    {
        ["R01.1"] = new(AuditQuestionResult.Yes),
        ["R01.2"] = new(AuditQuestionResult.No, "Eine Vertretung ist nachweislich nicht benannt."),
        ["R02.1"] = new(AuditQuestionResult.Yes),
        ["R02.2"] = new(AuditQuestionResult.Yes),
        ["R03.1"] = new(AuditQuestionResult.Yes),
        ["R03.2"] = new(AuditQuestionResult.No, "Für eine Berechtigung wurde nachweislich noch keine Freigabe erteilt."),
        ["R04.1"] = new(AuditQuestionResult.Yes),
        ["R04.2"] = new(AuditQuestionResult.Yes),
        ["R05.1"] = new(AuditQuestionResult.Yes),
        ["R05.2"] = new(AuditQuestionResult.Yes),
        ["R06.1"] = new(AuditQuestionResult.NotDeterminable, "Die Freigabeunterlagen sind zum Prüfzeitpunkt nicht zugänglich; der Sachverhalt lässt sich nicht verlässlich feststellen."),
        ["R06.2"] = new(AuditQuestionResult.Yes),
        ["R07.1"] = new(AuditQuestionResult.No, "Ein abgestellter Wartungswagen blockiert einen bezeichneten Zugang."),
        ["R07.2"] = new(AuditQuestionResult.Yes),
        ["R08.1"] = new(AuditQuestionResult.Yes),
        ["R08.2"] = new(AuditQuestionResult.Yes),
        ["R09.1"] = new(AuditQuestionResult.Yes),
        ["R09.2"] = new(AuditQuestionResult.Yes),
        ["R10.1"] = new(AuditQuestionResult.NotApplicable, "Im Raum ist keine solche Sensorik installiert; die bedingte Anforderung greift nicht."),
        ["R10.2"] = new(AuditQuestionResult.NotApplicable, "Im Raum ist keine solche Sensorik installiert; die bedingte Anforderung greift nicht.")
    };

    private static readonly IReadOnlyDictionary<string, Answer> RepeatAnswers = new Dictionary<string, Answer>(FirstAnswers)
    {
        ["R01.2"] = new(AuditQuestionResult.Yes),
        ["R03.2"] = new(AuditQuestionResult.Yes),
        ["R06.1"] = new(AuditQuestionResult.Yes),
        ["R07.1"] = new(AuditQuestionResult.Yes)
    };
}
