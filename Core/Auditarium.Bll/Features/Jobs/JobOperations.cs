// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Jobs;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Common.Time;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Jobs;

public sealed record JobOperationsItem(
    string JobKey,
    bool Enabled,
    JobTrigger AllowedTriggers,
    string? Schedule,
    string TimeZone,
    bool IsRunning,
    DateTimeOffset? LastStartedAt,
    DateTimeOffset? LastCompletedAt,
    string? LastResult,
    string? RunningInstanceId,
    DateTimeOffset? NextRun);

[RequiresPermission("Maintenance.Jobs.Execute")]
public sealed record ListJobOperationsQuery() : IRequest<Result<IReadOnlyList<JobOperationsItem>>>;

[RequiresPermission("Maintenance.Jobs.Execute")]
public sealed record TriggerJobCommand(string JobKey) : IRequest<Result>;

public sealed class JobOperationsHandler(
    IJobRegistry registry,
    JobConfigurationProvider configurationProvider,
    IAuditariumDbContext db,
    IClock clock,
    IJobCoordinator coordinator,
    IAuditEventWriter auditEvents) :
    IRequestHandler<ListJobOperationsQuery, Result<IReadOnlyList<JobOperationsItem>>>,
    IRequestHandler<TriggerJobCommand, Result>
{
    public async ValueTask<Result<IReadOnlyList<JobOperationsItem>>> Handle(ListJobOperationsQuery query, CancellationToken ct)
    {
        var configurations = await configurationProvider.GetAllAsync(ct);
        var states = await db.JobRuntimeStates.AsNoTracking().ToDictionaryAsync(x => x.JobKey, ct);
        var now = clock.UtcNow;
        var items = configurations.Select(configuration =>
        {
            states.TryGetValue(configuration.Definition.JobKey, out var state);
            var isRunning = state is { RunningInstanceId: not null, LeaseUntil: not null } && state.LeaseUntil > now;
            return new JobOperationsItem(
                configuration.Definition.JobKey,
                configuration.Enabled,
                configuration.Definition.AllowedTriggers,
                configuration.Schedule,
                configuration.TimeZone.Id,
                isRunning,
                state?.LastStartedAt,
                state?.LastCompletedAt,
                state?.LastResult,
                isRunning ? state!.RunningInstanceId : null,
                JobScheduleCalculator.GetNextRun(configuration, now));
        }).OrderBy(x => x.JobKey, StringComparer.Ordinal).ToArray();
        return Result<IReadOnlyList<JobOperationsItem>>.Success(items);
    }

    public async ValueTask<Result> Handle(TriggerJobCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.JobKey) || !registry.TryGet(command.JobKey, out var definition) || definition is null)
            return Result.Failure(new AppError("JOB.NOT_FOUND", ErrorType.NotFound));

        var result = await coordinator.RunAsync(definition.JobKey, JobTrigger.Manual, ct);
        if (!result.IsSuccess) return result;

        await auditEvents.WriteAsync(new AuditEvent(
            "JOB_TRIGGERED",
            "MaintenanceJob",
            AfterState: new Dictionary<string, object?>
            {
                ["job_key"] = definition.JobKey,
                ["trigger"] = "MANUAL"
            }), ct);
        return Result.Success();
    }
}
