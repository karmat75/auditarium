// SPDX-License-Identifier: MIT
using System.Text.Json;
using Auditarium.Bll.Features.Audits;
using Auditarium.Dal;
using Auditarium.DemoData;
using Auditarium.Models.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

public sealed class DemoFixtureIntegrationTests
{
    [Fact]
    public async Task PostgreSql_creates_the_complete_fixture_and_a_repeat_apply_is_a_no_op()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();

        var snapshot = await ApplyAndAssertAsync("PostgreSQL", container.GetConnectionString());

        Assert.Equal(DemoFixtureDefinition.FixtureVersion, snapshot.FixtureVersion);
    }

    [Fact]
    public async Task SqlServer_creates_the_complete_fixture()
    {
        await using var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await container.StartAsync();

        var snapshot = await ApplyAndAssertAsync("SqlServer", container.GetConnectionString());

        Assert.Equal(DemoFixtureDefinition.FixtureVersion, snapshot.FixtureVersion);
    }

    [Fact]
    public async Task PostgreSql_and_sql_server_have_the_same_logical_fixture_snapshot()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await using var sqlServer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await postgres.StartAsync();
        await sqlServer.StartAsync();

        var postgresSnapshot = await ApplyAndAssertAsync("PostgreSQL", postgres.GetConnectionString());
        var sqlServerSnapshot = await ApplyAndAssertAsync("SqlServer", sqlServer.GetConnectionString());

        AssertSnapshotsEqual(postgresSnapshot, sqlServerSnapshot);
    }

    [Fact]
    public async Task Unknown_business_data_is_rejected_before_a_fixture_marker_is_created()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var services = CreateProvider("PostgreSQL", container.GetConnectionString());
        await services.InitializeAuditariumDatabaseAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        db.Documents.Add(new Document { Title = "Unbekanntes Regelwerk", UsageState = DocumentUsageState.Active });
        await db.SaveChangesAsync();
        var roleCountBefore = await db.UserRoles.CountAsync();

        var exception = await Assert.ThrowsAsync<DemoDataFixtureException>(() => scope.ServiceProvider.GetRequiredService<DemoFixtureRunner>().ApplyAsync());

        Assert.Contains("Unknown business data", exception.Message, StringComparison.Ordinal);
        Assert.Equal(1, await db.Documents.CountAsync());
        Assert.Empty(await db.Documents.Where(document => document.Source == DemoFixtureDefinition.MarkerSource).ToListAsync());
        Assert.Equal(roleCountBefore, await db.UserRoles.CountAsync());
    }

    [Fact]
    public async Task Failed_run_leaves_a_visible_incomplete_marker_and_refuses_to_continue()
    {
        await using var container = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await container.StartAsync();
        await using var services = CreateProvider("PostgreSQL", container.GetConnectionString());
        await services.InitializeAuditariumDatabaseAsync();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditariumDbContext>();
        await PrepareActorAsync(scope.ServiceProvider);
        var runner = scope.ServiceProvider.GetRequiredService<DemoFixtureRunner>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.ApplyAsync(afterBuildingMarker: _ => Task.FromException(new InvalidOperationException("injected failure"))));
        db.ChangeTracker.Clear();
        var marker = await db.Documents.SingleAsync(document => document.Source == DemoFixtureDefinition.MarkerSource);
        Assert.Equal(DemoFixtureDefinition.BuildingMarker, marker.Notes);

        var exception = await Assert.ThrowsAsync<DemoDataFixtureException>(() => runner.ApplyAsync());
        Assert.Contains("incomplete", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, await db.Documents.CountAsync());
        Assert.Empty(await db.AuditUnits.ToListAsync());
    }

    [Fact]
    public void Fixture_runner_uses_mediator_commands_and_never_instantiates_handlers_or_adds_business_entities_directly()
    {
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(root, "Tools", "Auditarium.DemoData", "DemoFixtureRunner.cs"));

        Assert.Contains("mediator.Send", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new CatalogCommandHandler", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new AuditCommandHandler", source, StringComparison.Ordinal);
        Assert.DoesNotContain("db.Documents.Add", source, StringComparison.Ordinal);
        Assert.DoesNotContain("db.AuditUnits.Add", source, StringComparison.Ordinal);
        Assert.DoesNotContain("db.Audits.Add", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteSql", source, StringComparison.Ordinal);
    }

    private static async Task<FixtureSnapshot> ApplyAndAssertAsync(string providerName, string connectionString)
    {
        await using var services = CreateProvider(providerName, connectionString);
        await services.InitializeAuditariumDatabaseAsync();
        await using var scope = services.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var db = serviceProvider.GetRequiredService<AuditariumDbContext>();
        await PrepareActorAsync(serviceProvider);
        var runner = serviceProvider.GetRequiredService<DemoFixtureRunner>();

        var applied = await runner.ApplyAsync();
        Assert.Equal(DemoFixtureApplyStatus.Applied, applied.Status);
        var snapshot = await CreateSnapshotAsync(db);
        AssertFixture(db, snapshot);

        var auditLogsBefore = await db.SystemAuditLogs.CountAsync();
        var reapply = await runner.ApplyAsync();
        Assert.Equal(DemoFixtureApplyStatus.AlreadyPresent, reapply.Status);
        Assert.Equal(auditLogsBefore, await db.SystemAuditLogs.CountAsync());
        AssertSnapshotsEqual(snapshot, await CreateSnapshotAsync(db));
        return snapshot;
    }

    private static async Task PrepareActorAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AuditariumDbContext>();
        var defaultAdministratorId = await new DevelopmentAdministratorProvisioner(db).ProvisionAsync();
        services.GetRequiredService<DemoDataCurrentActor>().SetDefaultAdministrator(defaultAdministratorId);
    }

    private static void AssertFixture(AuditariumDbContext db, FixtureSnapshot snapshot)
    {
        Assert.Equal(DemoFixtureDefinition.FixtureVersion, snapshot.FixtureVersion);
        var auditStates = snapshot.Audits.Select(audit => audit.State).Distinct().ToArray();
        Assert.Equal(5, auditStates.Length);
        Assert.All(Enum.GetValues<AuditState>(), state => Assert.Contains(state, auditStates));
        Assert.Contains(snapshot.Units, unit => unit.Name == DemoFixtureDefinition.RetiredRoomName && unit.State == AuditUnitUsageState.Inactive);
        Assert.Contains(snapshot.Units, unit => unit.Name == DemoFixtureDefinition.ServerRoomName && unit.State == AuditUnitUsageState.Active);
        Assert.Equal([CatalogState.Ready, CatalogState.Draft], snapshot.Catalogs.Select(catalog => catalog.State).ToArray());
        Assert.Contains(snapshot.Audits, audit => audit.Name == DemoFixtureDefinition.RepeatAuditName && audit.Origin == DemoFixtureDefinition.InitialAuditName && !audit.Assigned);
        Assert.Contains(snapshot.Audits, audit => audit.Name == DemoFixtureDefinition.InProgressAuditName && audit.Assigned);
        Assert.Contains(snapshot.Audits, audit => audit.Name == DemoFixtureDefinition.InitialAuditName && audit.State == AuditState.Finalized);
        var answerResults = snapshot.Answers.Select(answer => answer.Result).Distinct().ToArray();
        Assert.Equal(4, answerResults.Length);
        Assert.All(Enum.GetValues<AuditQuestionResult>(), result => Assert.Contains(result, answerResults));
        Assert.Contains(snapshot.Answers, answer => answer.Result == AuditQuestionResult.No && answer.HasComment);
        Assert.Contains(snapshot.Answers, answer => answer.Result == AuditQuestionResult.NotDeterminable && answer.HasComment && answer.HasEvidence);
        Assert.True(db.SystemAuditLogs.Any(log => log.ObjectType == nameof(Audit)));
        Assert.False((from userRole in db.UserRoles
                      join role in db.Roles on userRole.RoleId equals role.RoleId
                      join user in db.Users on userRole.UserId equals user.UserId
                      where user.UserKey == "DEFAULT_ADMIN"
                      select role.RoleKey).Contains("SYSTEM_INTERNAL"));
        var finalized = db.Audits.Single(audit => audit.Name == DemoFixtureDefinition.InitialAuditName);
        var settings = JsonSerializer.Deserialize<AuditSettings>(finalized.AuditSettings)!;
        Assert.True(settings.ResponsePolicy[AuditQuestionResult.No].CommentRequired);
        Assert.True(settings.ResponsePolicy[AuditQuestionResult.NotDeterminable].EvidenceRequired);
    }

    private static async Task<FixtureSnapshot> CreateSnapshotAsync(AuditariumDbContext db)
    {
        var units = await (from unit in db.AuditUnits
                           join scope in db.ScopeTypes on unit.ScopeTypeId equals scope.ScopeTypeId
                           select new { unit, scope.Key }).ToListAsync();
        var unitNames = units.ToDictionary(unit => unit.unit.AuditUnitId, unit => unit.unit.Name);
        var audits = (await db.Audits.ToListAsync())
            .OrderBy(audit => audit.Name, StringComparer.Ordinal)
            .ToList();
        var auditNames = audits.ToDictionary(audit => audit.AuditId, audit => audit.Name);
        var answers = (await (from question in db.AuditQuestions
                              join auditElement in db.AuditDocumentElements on question.AuditDocumentElementId equals auditElement.AuditDocumentElementId
                              join element in db.DocumentElements on auditElement.ElementId equals element.ElementId
                              join audit in db.Audits on auditElement.AuditId equals audit.AuditId
                              where question.Result != null
                              select new FixtureAnswer(audit.Name, element.Title!, question.Result!.Value, question.Comment != null, question.Evidence != null)).ToListAsync())
            .OrderBy(answer => answer.Audit, StringComparer.Ordinal)
            .ThenBy(answer => answer.Reference, StringComparer.Ordinal)
            .ToList();
        var catalogs = await db.CatalogVersions.OrderBy(catalog => catalog.VersionNumber).ToListAsync();
        return new(
            DemoFixtureDefinition.FixtureVersion,
            units.Select(unit => new FixtureUnit(unit.unit.Name, unit.Key, unit.unit.UsageState, unit.unit.ParentAuditUnitId is null ? null : unitNames[unit.unit.ParentAuditUnitId.Value])).OrderBy(unit => unit.Name).ToArray(),
            catalogs.Select(catalog => new FixtureCatalog(catalog.VersionNumber, catalog.CatalogState)).ToArray(),
            audits.Select(audit => new FixtureAudit(audit.Name, audit.AuditState, audit.AssignedAuditorUserId is not null, audit.OriginAuditId is null ? null : auditNames[audit.OriginAuditId.Value])).ToArray(),
            answers);
    }

    private static void AssertSnapshotsEqual(FixtureSnapshot expected, FixtureSnapshot actual)
    {
        Assert.Equal(expected.FixtureVersion, actual.FixtureVersion);
        Assert.Equal(expected.Units, actual.Units);
        Assert.Equal(expected.Catalogs, actual.Catalogs);
        Assert.Equal(expected.Audits, actual.Audits);
        Assert.Equal(expected.Answers, actual.Answers);
    }

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
        services.AddAuditariumDemoData(configuration);
        return services.BuildServiceProvider();
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Auditarium.sln"))) return directory.FullName;
        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private sealed record FixtureSnapshot(int FixtureVersion, FixtureUnit[] Units, FixtureCatalog[] Catalogs, FixtureAudit[] Audits, List<FixtureAnswer> Answers);
    private sealed record FixtureUnit(string Name, string ScopeKey, AuditUnitUsageState State, string? Parent);
    private sealed record FixtureCatalog(int Version, CatalogState State);
    private sealed record FixtureAudit(string Name, AuditState State, bool Assigned, string? Origin);
    private sealed record FixtureAnswer(string Audit, string Reference, AuditQuestionResult Result, bool HasComment, bool HasEvidence);
}
