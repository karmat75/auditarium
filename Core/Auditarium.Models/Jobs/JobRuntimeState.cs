// SPDX-License-Identifier: MIT
namespace Auditarium.Models.Jobs;

/// <summary>
/// The current and most recently completed distributed execution state of a code-defined job.
/// This is operational state, not operator-configurable job configuration or execution history.
/// </summary>
public sealed class JobRuntimeState
{
    public required string JobKey { get; set; }
    public DateTimeOffset? LastStartedAt { get; set; }
    public DateTimeOffset? LastCompletedAt { get; set; }
    public string? LastResult { get; set; }
    public string? RunningInstanceId { get; set; }
    public DateTimeOffset? LeaseUntil { get; set; }
    public string? LastStartupVersion { get; set; }
    public long ConcurrencyVersion { get; set; } = 1;
}
