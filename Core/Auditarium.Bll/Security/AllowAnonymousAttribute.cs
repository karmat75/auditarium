// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class AllowAnonymousAttribute : Attribute;
