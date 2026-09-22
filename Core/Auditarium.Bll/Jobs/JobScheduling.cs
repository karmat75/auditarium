// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Settings;
using Auditarium.Bll.Settings;
using Auditarium.Common.Time;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auditarium.Bll.Jobs;

[Flags]
public enum JobTrigger
{
    None = 0,
    Scheduled = 1,
    Startup = 2,
    Manual = 4
}

public enum MisfirePolicy { Skip }
public enum JobConcurrencyPolicy { SkipIfRunning }

public sealed record JobDefinition(string JobKey, JobTrigger AllowedTriggers);

public static class JobKeys
{
    public const string Retention = "Retention";
}

public interface IJobRegistry
{
    IReadOnlyList<JobDefinition> Definitions { get; }
    bool TryGet(string jobKey, out JobDefinition? definition);
}

public sealed class JobRegistry : IJobRegistry
{
    public IReadOnlyList<JobDefinition> Definitions { get; } =
    [
        new(JobKeys.Retention, JobTrigger.Scheduled | JobTrigger.Manual)
    ];

    public bool TryGet(string jobKey, out JobDefinition? definition)
    {
        definition = Definitions.SingleOrDefault(x => string.Equals(x.JobKey, jobKey, StringComparison.Ordinal));
        return definition is not null;
    }
}

public sealed record JobConfiguration(
    JobDefinition Definition,
    bool Enabled,
    string? Schedule,
    TimeZoneInfo TimeZone,
    bool RunOnStartup,
    MisfirePolicy MisfirePolicy,
    JobConcurrencyPolicy ConcurrencyPolicy);

public static class JobSettingDefinitions
{
    public static IReadOnlyList<SettingDefinition> All { get; } =
    [
        new("Jobs:Retention:Enabled", "bool", "true", true, IsBoolean, Category: "Jobs: Retention", DisplayName: "Aktiviert", DisplayOrder: 100),
        new("Jobs:Retention:Schedule", "string", "0 3 * * *", true, IsSchedule, Category: "Jobs: Retention", DisplayName: "Zeitplan", DisplayOrder: 110),
        new("Jobs:Retention:TimeZone", "string", "Europe/Berlin", true, IsTimeZone, Category: "Jobs: Retention", DisplayName: "Zeitzone", DisplayOrder: 120),
        new("Jobs:Retention:RunOnStartup", "bool", "false", true, IsBoolean, Category: "Jobs: Retention", DisplayName: "Beim Start ausführen", DisplayOrder: 130),
        new("Jobs:Retention:MisfirePolicy", "string", "Skip", true, v => string.Equals(v, nameof(MisfirePolicy.Skip), StringComparison.OrdinalIgnoreCase), Category: "Jobs: Retention", DisplayName: "Misfire-Verhalten", DisplayOrder: 140, AllowedValues: [nameof(MisfirePolicy.Skip)]),
        new("Jobs:Retention:ConcurrencyPolicy", "string", "SkipIfRunning", true, v => string.Equals(v, nameof(JobConcurrencyPolicy.SkipIfRunning), StringComparison.OrdinalIgnoreCase), Category: "Jobs: Retention", DisplayName: "Parallelitätsverhalten", DisplayOrder: 150, AllowedValues: [nameof(JobConcurrencyPolicy.SkipIfRunning)])
    ];

    private static bool IsBoolean(string value) => bool.TryParse(value, out _);
    private static bool IsSchedule(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        try { _ = CronExpression.Parse(value, CronFormat.Standard); return true; }
        catch (CronFormatException) { return false; }
    }
    private static bool IsTimeZone(string value)
    {
        try { _ = TimeZoneInfo.FindSystemTimeZoneById(value); return true; }
        catch (TimeZoneNotFoundException) { return false; }
        catch (InvalidTimeZoneException) { return false; }
    }
}

public sealed class JobConfigurationProvider(IJobRegistry registry, IApplicationSettingResolver settings, IConfiguration configuration)
{
    public async Task<IReadOnlyList<JobConfiguration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var configurations = new List<JobConfiguration>(registry.Definitions.Count);
        foreach (var definition in registry.Definitions)
            configurations.Add(await GetAsync(definition.JobKey, cancellationToken));
        return configurations;
    }

    public async Task<JobConfiguration> GetAsync(string jobKey, CancellationToken cancellationToken = default)
    {
        if (!registry.TryGet(jobKey, out var definition) || definition is null) throw new KeyNotFoundException($"Unknown job '{jobKey}'.");
        var prefix = $"Jobs:{definition.JobKey}:";
        ValidateExternalValues(prefix);
        var enabled = await ValueAsync<bool>(prefix + "Enabled", cancellationToken);
        var schedule = await ValueAsync<string>(prefix + "Schedule", cancellationToken);
        var timeZoneId = await ValueAsync<string>(prefix + "TimeZone", cancellationToken);
        var runOnStartup = await ValueAsync<bool>(prefix + "RunOnStartup", cancellationToken);
        var misfire = await ValueAsync<MisfirePolicy>(prefix + "MisfirePolicy", cancellationToken);
        var concurrency = await ValueAsync<JobConcurrencyPolicy>(prefix + "ConcurrencyPolicy", cancellationToken);
        Validate(definition, schedule, runOnStartup);
        return new JobConfiguration(definition, enabled, string.IsNullOrWhiteSpace(schedule) ? null : schedule, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId), runOnStartup, misfire, concurrency);
    }

    public static void Validate(JobDefinition definition, string? schedule, bool runOnStartup)
    {
        if (runOnStartup && !definition.AllowedTriggers.HasFlag(JobTrigger.Startup))
            throw new InvalidOperationException($"Job '{definition.JobKey}' has RunOnStartup enabled although Startup is not an allowed trigger.");
        if (!string.IsNullOrWhiteSpace(schedule) && !definition.AllowedTriggers.HasFlag(JobTrigger.Scheduled))
            throw new InvalidOperationException($"Job '{definition.JobKey}' has a schedule although Scheduled is not an allowed trigger.");
    }

    private async Task<T> ValueAsync<T>(string key, CancellationToken ct)
    {
        var value = (await settings.GetAsync(key, ct)).Value;
        if (typeof(T).IsEnum) return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        return (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }

    private void ValidateExternalValues(string prefix)
    {
        foreach (var definition in JobSettingDefinitions.All.Where(x => x.Key.StartsWith(prefix, StringComparison.Ordinal)))
        {
            var environment = Environment.GetEnvironmentVariable("AUDITARIUM__" + definition.Key.Replace(":", "__", StringComparison.Ordinal));
            if (environment is not null && !definition.IsValid(environment))
                throw new InvalidOperationException($"Job setting '{definition.Key}' has an invalid environment value.");
            var configured = configuration["Auditarium:" + definition.Key];
            if (configured is not null && !definition.IsValid(configured))
                throw new InvalidOperationException($"Job setting '{definition.Key}' has an invalid configuration value.");
        }
    }
}

public static class JobScheduleCalculator
{
    public static DateTimeOffset? GetNextRun(JobConfiguration configuration, DateTimeOffset utcNow)
    {
        if (!configuration.Enabled || string.IsNullOrWhiteSpace(configuration.Schedule)) return null;
        var expression = CronExpression.Parse(configuration.Schedule, CronFormat.Standard);
        var next = expression.GetNextOccurrence(utcNow.UtcDateTime, configuration.TimeZone, inclusive: false);
        return next is null ? null : new DateTimeOffset(DateTime.SpecifyKind(next.Value, DateTimeKind.Utc));
    }
}

public sealed class JobScheduleBackgroundService(IServiceScopeFactory scopeFactory, IClock clock, ILogger<JobScheduleBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextRuns = await GetNextRunsAsync(stoppingToken);
            var nextRun = nextRuns.Where(x => x.NextRun is not null).Select(x => x.NextRun!.Value).DefaultIfEmpty(clock.UtcNow.AddMinutes(1)).Min();
            var delay = nextRun - clock.UtcNow;
            if (delay <= TimeSpan.Zero) delay = TimeSpan.FromSeconds(1);
            await Task.Delay(delay, stoppingToken);
            // Execution is deliberately introduced by JobCoordinator in work package 20.9.2.
        }
    }

    private async Task<IReadOnlyList<(string JobKey, DateTimeOffset? NextRun)>> GetNextRunsAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var configurations = await scope.ServiceProvider.GetRequiredService<JobConfigurationProvider>().GetAllAsync(ct);
        var now = clock.UtcNow;
        var nextRuns = configurations.Select(x => (x.Definition.JobKey, JobScheduleCalculator.GetNextRun(x, now))).ToArray();
        foreach (var (jobKey, nextRun) in nextRuns)
            logger.LogDebug("Job {JobKey} next scheduled trigger is {NextRun}.", jobKey, nextRun);
        return nextRuns;
    }
}
