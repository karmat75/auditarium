// SPDX-License-Identifier: MIT
using Auditarium.Bll.Jobs;
using Auditarium.Bll.Features.Jobs;
using Auditarium.Bll.Settings;
using Xunit;

namespace Auditarium.Common.Tests;

public sealed class JobSchedulingTests
{
    [Fact]
    public void Registry_ResolvesRetentionWithCodeDefinedTriggers()
    {
        var registry = new JobRegistry();

        Assert.True(registry.TryGet(JobKeys.Retention, out var job));
        Assert.Equal(JobTrigger.Scheduled | JobTrigger.Manual, job!.AllowedTriggers);
    }

    [Fact]
    public void NextRun_IsCalculatedFromCronTimeZoneAndUtcNow()
    {
        var configuration = new JobConfiguration(new(JobKeys.Retention, JobTrigger.Scheduled), true, "0 3 * * *", TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin"), false, MisfirePolicy.Skip, JobConcurrencyPolicy.SkipIfRunning);

        var next = JobScheduleCalculator.GetNextRun(configuration, new DateTimeOffset(2026, 1, 15, 2, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateTimeOffset(2026, 1, 16, 2, 0, 0, TimeSpan.Zero), next);
    }

    [Fact]
    public void NextRun_SkipsMissedOccurrence()
    {
        var configuration = new JobConfiguration(new(JobKeys.Retention, JobTrigger.Scheduled), true, "0 3 * * *", TimeZoneInfo.Utc, false, MisfirePolicy.Skip, JobConcurrencyPolicy.SkipIfRunning);

        var next = JobScheduleCalculator.GetNextRun(configuration, new DateTimeOffset(2026, 1, 15, 8, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateTimeOffset(2026, 1, 16, 3, 0, 0, TimeSpan.Zero), next);
    }

    [Fact]
    public void Validation_RejectsConfigurationForDisallowedTrigger()
    {
        var job = new JobDefinition("ScheduledOnly", JobTrigger.Scheduled);

        Assert.Throws<InvalidOperationException>(() => JobConfigurationProvider.Validate(job, "0 3 * * *", true));
    }

    [Fact]
    public void ScheduleValidation_UsesFiveFieldCronWithoutSeconds()
    {
        var schedule = JobSettingDefinitions.All.Single(x => x.Key == "Jobs:Retention:Schedule");

        Assert.True(schedule.IsValid("0 3 * * *"));
        Assert.False(schedule.IsValid("0 0 3 * * *"));
    }

    [Fact]
    public void Retention_factory_maps_only_the_retention_job_to_its_bll_command()
    {
        var factory = new RetentionJobExecutionRequestFactory();

        Assert.True(factory.TryCreate(JobKeys.Retention, out var request));
        Assert.IsType<RunRetentionCommand>(request);
        Assert.False(factory.TryCreate("FileIntegrity", out request));
        Assert.Null(request);
    }

}
