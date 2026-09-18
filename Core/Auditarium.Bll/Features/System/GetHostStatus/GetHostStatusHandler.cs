// SPDX-License-Identifier: MIT
using Auditarium.Common.Results;
using Mediator;

namespace Auditarium.Bll.Features.System.GetHostStatus;

public sealed class GetHostStatusHandler : IRequestHandler<GetHostStatusQuery, Result<HostStatusViewModel>>
{
    public ValueTask<Result<HostStatusViewModel>> Handle(GetHostStatusQuery message, CancellationToken cancellationToken)
        => ValueTask.FromResult(Result<HostStatusViewModel>.Success(new("ready")));
}
