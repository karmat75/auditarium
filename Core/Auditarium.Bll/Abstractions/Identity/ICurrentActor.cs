// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Abstractions.Identity;

public interface ICurrentActor
{
    ActorType Type { get; }
    long? UserId { get; }
    bool IsAuthenticated { get; }
    bool MustChangePassword => false;
}
