// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class RequiresPermissionAttribute(params string[] permissions) : Attribute
{
    public IReadOnlyList<string> Permissions { get; } = permissions.Length > 0
        ? permissions
        : throw new ArgumentException("At least one permission is required.", nameof(permissions));
}
