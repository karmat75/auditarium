// SPDX-License-Identifier: MIT
namespace Auditarium.Common.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
