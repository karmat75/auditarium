// SPDX-License-Identifier: MIT
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Mediator;

namespace Auditarium.Bll.Features.System.GetHostStatus;

[AllowAnonymous]
public sealed record GetHostStatusQuery : IRequest<Result<HostStatusViewModel>>;

public sealed record HostStatusViewModel(string Status);
