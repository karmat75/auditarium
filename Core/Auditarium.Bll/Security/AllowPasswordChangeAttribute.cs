// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Security;

/// <summary>
/// Marks the narrowly scoped self-service password-change operation. It remains
/// available to an authenticated user whose temporary LOCAL credential requires
/// a password change, without granting a regular application permission.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class AllowPasswordChangeAttribute : Attribute;
