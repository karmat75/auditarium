// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Models.Identity;
using System.Text.Json;

namespace Auditarium.Dal;

internal sealed class AuditEventWriter(AuditariumDbContext db, ICurrentActor actor) : IAuditEventWriter
{
    public async Task WriteAsync(AuditEvent eventData, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventData.Action);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventData.ObjectType);
        AuditLogContract.Validate(eventData.Action, eventData.ObjectType);
        db.SystemAuditLogs.Add(new SystemAuditLog
        {
            OccurredAt = DateTimeOffset.UtcNow,
            UserId = eventData.ActorUserId ?? actor.UserId ?? 0,
            Action = eventData.Action,
            ObjectType = eventData.ObjectType,
            ObjectId = eventData.ObjectId,
            BeforeState = null,
            AfterState = eventData.AfterState is { Count: > 0 } ? JsonSerializer.Serialize(eventData.AfterState) : null
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
