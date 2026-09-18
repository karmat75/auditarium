// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Abstractions.Identity;

public interface IPermissionEvaluator
{
    Task<bool> HasPermissionAsync(long userId, string permission, CancellationToken cancellationToken = default);
    Task<IReadOnlySet<string>> GetPermissionsAsync(long userId, CancellationToken cancellationToken = default);
}
