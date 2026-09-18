// SPDX-License-Identifier: MIT
namespace Auditarium.Common.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
