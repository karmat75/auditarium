// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;

namespace Auditarium.DemoData;

/// <summary>Scoped authenticated-user actor used solely by the DemoData console tool.</summary>
public sealed class DemoDataCurrentActor : ICurrentActor
{
    private long? userId;

    public ActorType Type => ActorType.User;
    public long? UserId => userId;
    public bool IsAuthenticated => userId is not null;

    public void SetDefaultAdministrator(long defaultAdministratorUserId) => userId = defaultAdministratorUserId;
}
