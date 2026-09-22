// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Settings;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Settings;
using Auditarium.Common.Results;
using Auditarium.Common.Time;
using Auditarium.Models.Jobs;
using Cronos;
using Mediator;
using Microsoft.EntityFrameworkCore;
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

/// <summary>Marker for the normal BLL command of a concrete job.</summary>
public interface IJobExecutionRequest : IRequest<Result> { }

public interface IJobExecutionRequestFactory
{
    bool TryCreate(string jobKey, out IJobExecutionRequest? request);
}

public interface IJobCoordinator
{
    Task<Result> RunAsync(string jobKey, JobTrigger trigger, CancellationToken cancellationToken = default);
}

public interface IJobInstanceIdentity { string Value { get; } }
public sealed class JobInstanceIdentity : IJobInstanceIdentity { public string Value { get; } = Guid.NewGuid().ToString("N"); }

public interface IApplicationVersion { string Value { get; } }
public sealed class ApplicationVersion : IApplicationVersion { public string Value { get; } = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0"; }

public interface ISystemExecutionContext { bool IsActive { get; } IDisposable Enter(); }

public sealed class SystemExecutionContext : ISystemExecutionContext
{
    private readonly AsyncLocal<int> _depth = new();
    public bool IsActive => _depth.Value > 0;
    public IDisposable Enter()
    {
        _depth.Value++;
        return new Scope(this);
    }

    private sealed class Scope(SystemExecutionContext context) : IDisposable
    {
        private SystemExecutionContext? _context = context;
        public void Dispose()
        {
            if (_context is not { } context) return;
            context._depth.Value--;
            _context = null;
        }
    }
}

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

public sealed class JobRuntimeStateStore(IAuditariumDbContext db, IClock clock)
{
    public static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan HeartbeatInterval = TimeSpan.FromMinutes(1);

    public async Task<bool> TryAcquireAsync(string jobKey, string instanceId, JobTrigger trigger, string applicationVersion, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var leaseUntil = now.Add(LeaseDuration);
        var states = db.JobRuntimeStates.Where(state => state.JobKey == jobKey &&
            (state.RunningInstanceId == null || state.LeaseUntil == null || state.LeaseUntil <= now) &&
            (trigger != JobTrigger.Startup || state.LastStartupVersion == null || state.LastStartupVersion != applicationVersion));
        var updated = await states.ExecuteUpdateAsync(setters => setters
            .SetProperty(state => state.RunningInstanceId, instanceId)
            .SetProperty(state => state.LeaseUntil, leaseUntil)
            .SetProperty(state => state.LastStartedAt, now)
            .SetProperty(state => state.ConcurrencyVersion, state => state.ConcurrencyVersion + 1), ct);
        if (updated != 0) return true;

        if (await db.JobRuntimeStates.AnyAsync(state => state.JobKey == jobKey, ct)) return false;
        db.JobRuntimeStates.Add(new JobRuntimeState { JobKey = jobKey, RunningInstanceId = instanceId, LeaseUntil = leaseUntil, LastStartedAt = now });
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            // A competing instance created the same key with a valid lease. Do not retry or overwrite it.
            return false;
        }
    }

    public async Task<bool> HeartbeatAsync(string jobKey, string instanceId, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var updated = await db.JobRuntimeStates.Where(state => state.JobKey == jobKey && state.RunningInstanceId == instanceId && state.LeaseUntil != null && state.LeaseUntil > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(state => state.LeaseUntil, now.Add(LeaseDuration))
                .SetProperty(state => state.ConcurrencyVersion, state => state.ConcurrencyVersion + 1), ct);
        return updated != 0;
    }

    public async Task CompleteAsync(string jobKey, string instanceId, JobTrigger trigger, string applicationVersion, bool succeeded, string result, CancellationToken ct)
    {
        var completedAt = clock.UtcNow;
        await db.JobRuntimeStates.Where(state => state.JobKey == jobKey && state.RunningInstanceId == instanceId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(state => state.LastCompletedAt, completedAt)
                .SetProperty(state => state.LastResult, result)
                .SetProperty(state => state.RunningInstanceId, (string?)null)
                .SetProperty(state => state.LeaseUntil, (DateTimeOffset?)null)
                .SetProperty(state => state.LastStartupVersion, state => trigger == JobTrigger.Startup && succeeded ? applicationVersion : state.LastStartupVersion)
                .SetProperty(state => state.ConcurrencyVersion, state => state.ConcurrencyVersion + 1), ct);
    }
}

public sealed class JobCoordinator(
    IServiceScopeFactory scopeFactory,
    IJobRegistry registry,
    IJobInstanceIdentity instanceIdentity,
    IApplicationVersion applicationVersion,
    ISystemExecutionContext systemExecutionContext,
    ILogger<JobCoordinator> logger) : IJobCoordinator
{
    public async Task<Result> RunAsync(string jobKey, JobTrigger trigger, CancellationToken cancellationToken = default)
    {
        if (!registry.TryGet(jobKey, out var definition) || definition is null) return Failure("JOB.NOT_FOUND", ErrorType.NotFound);
        if (trigger is JobTrigger.None or not (JobTrigger.Scheduled or JobTrigger.Startup or JobTrigger.Manual) || !definition.AllowedTriggers.HasFlag(trigger)) return Failure("JOB.TRIGGER_NOT_ALLOWED", ErrorType.Validation);

        await using var scope = scopeFactory.CreateAsyncScope();
        var configuration = await scope.ServiceProvider.GetRequiredService<JobConfigurationProvider>().GetAsync(jobKey, cancellationToken);
        if (!configuration.Enabled) return Failure("JOB.DISABLED", ErrorType.Conflict);
        if (trigger == JobTrigger.Startup && !configuration.RunOnStartup) return Failure("JOB.TRIGGER_NOT_ALLOWED", ErrorType.Validation);

        var state = scope.ServiceProvider.GetRequiredService<JobRuntimeStateStore>();
        if (!await state.TryAcquireAsync(jobKey, instanceIdentity.Value, trigger, applicationVersion.Value, cancellationToken)) return Failure("JOB.ALREADY_RUNNING", ErrorType.Conflict);
        _ = ExecuteAsync(jobKey, trigger, cancellationToken);
        return Result.Success();
    }

    private async Task ExecuteAsync(string jobKey, JobTrigger trigger, CancellationToken cancellationToken)
    {
        using var activity = JobTelemetry.ActivitySource.StartActivity("job.run");
        activity?.SetTag("job.key", jobKey);
        activity?.SetTag("job.trigger", trigger.ToString());
        JobTelemetry.RunsStarted.Add(1, new KeyValuePair<string, object?>("job.key", jobKey));
        var succeeded = false;
        var result = "Failed";
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var factory = scope.ServiceProvider.GetService<IJobExecutionRequestFactory>();
            if (factory is null || !factory.TryCreate(jobKey, out var request) || request is null)
            {
                result = "NoExecutionRegistered";
                logger.LogWarning("Job {JobKey} has no registered BLL execution request.", jobKey);
                return;
            }

            using var systemScope = systemExecutionContext.Enter();
            await using var heartbeat = KeepLeaseAliveAsync(jobKey, cancellationToken);
            var response = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(request, cancellationToken);
            succeeded = response.IsSuccess;
            result = succeeded ? "Succeeded" : response.Errors.FirstOrDefault()?.Code ?? "Failed";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            result = "Cancelled";
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Job {JobKey} failed unexpectedly.", jobKey);
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, exception.Message);
        }
        finally
        {
            try
            {
                await using var completionScope = scopeFactory.CreateAsyncScope();
                await completionScope.ServiceProvider.GetRequiredService<JobRuntimeStateStore>().CompleteAsync(jobKey, instanceIdentity.Value, trigger, applicationVersion.Value, succeeded, result, CancellationToken.None);
                JobTelemetry.RunsCompleted.Add(1, new KeyValuePair<string, object?>("job.key", jobKey), new KeyValuePair<string, object?>("job.result", result));
            }
            catch (Exception exception) { logger.LogError(exception, "Job {JobKey} could not persist its completion state; the lease will recover after expiry.", jobKey); }
        }
    }

    private IAsyncDisposable KeepLeaseAliveAsync(string jobKey, CancellationToken cancellationToken)
    {
        var heartbeatCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var task = Task.Run(async () =>
        {
            try
            {
                using var timer = new PeriodicTimer(JobRuntimeStateStore.HeartbeatInterval);
                while (await timer.WaitForNextTickAsync(heartbeatCancellation.Token))
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    if (!await scope.ServiceProvider.GetRequiredService<JobRuntimeStateStore>().HeartbeatAsync(jobKey, instanceIdentity.Value, heartbeatCancellation.Token))
                    {
                        logger.LogWarning("Job {JobKey} lost its lease; its completion state will not overwrite another instance.", jobKey);
                        return;
                    }
                }
            }
            catch (OperationCanceledException) when (heartbeatCancellation.IsCancellationRequested) { }
        }, CancellationToken.None);
        return new HeartbeatLifetime(heartbeatCancellation, task);
    }

    private static Result Failure(string code, ErrorType type) => Result.Failure(new AppError(code, type));

    private sealed class HeartbeatLifetime(CancellationTokenSource cancellation, Task task) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await cancellation.CancelAsync();
            try { await task; }
            finally { cancellation.Dispose(); }
        }
    }
}

internal static class JobTelemetry
{
    internal static readonly System.Diagnostics.ActivitySource ActivitySource = new("Auditarium");
    internal static readonly System.Diagnostics.Metrics.Meter Meter = new("Auditarium");
    internal static readonly System.Diagnostics.Metrics.Counter<long> RunsStarted = Meter.CreateCounter<long>("auditarium.jobs.started");
    internal static readonly System.Diagnostics.Metrics.Counter<long> RunsCompleted = Meter.CreateCounter<long>("auditarium.jobs.completed");
}

public sealed class JobScheduleBackgroundService(IServiceScopeFactory scopeFactory, IJobCoordinator coordinator, IClock clock, ILogger<JobScheduleBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await TriggerStartupJobsAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextRuns = await GetNextRunsAsync(stoppingToken);
            var nextRun = nextRuns.Where(x => x.NextRun is not null).Select(x => x.NextRun!.Value).DefaultIfEmpty(clock.UtcNow.AddMinutes(1)).Min();
            var delay = nextRun - clock.UtcNow;
            if (delay <= TimeSpan.Zero) delay = TimeSpan.FromSeconds(1);
            await Task.Delay(delay, stoppingToken);
            foreach (var scheduled in nextRuns.Where(x => x.NextRun == nextRun))
            {
                var result = await coordinator.RunAsync(scheduled.JobKey, JobTrigger.Scheduled, stoppingToken);
                if (!result.IsSuccess) logger.LogInformation("Scheduled trigger for job {JobKey} was not accepted: {Code}.", scheduled.JobKey, result.Errors.FirstOrDefault()?.Code);
            }
        }
    }

    private async Task TriggerStartupJobsAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var configurations = await scope.ServiceProvider.GetRequiredService<JobConfigurationProvider>().GetAllAsync(ct);
        foreach (var configuration in configurations.Where(x => x.Enabled && x.RunOnStartup && x.Definition.AllowedTriggers.HasFlag(JobTrigger.Startup)))
        {
            var result = await coordinator.RunAsync(configuration.Definition.JobKey, JobTrigger.Startup, ct);
            if (!result.IsSuccess) logger.LogInformation("Startup trigger for job {JobKey} was not accepted: {Code}.", configuration.Definition.JobKey, result.Errors.FirstOrDefault()?.Code);
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
