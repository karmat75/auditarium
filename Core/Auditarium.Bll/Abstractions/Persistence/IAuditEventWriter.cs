// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Abstractions.Persistence;

/// <summary>Explicit event path for security events without an entity change.</summary>
public interface IAuditEventWriter
{
    Task WriteAsync(AuditEvent eventData, CancellationToken cancellationToken = default);
}

public sealed record AuditEvent(
    string Action,
    string ObjectType,
    long? ObjectId = null,
    long? ActorUserId = null);
