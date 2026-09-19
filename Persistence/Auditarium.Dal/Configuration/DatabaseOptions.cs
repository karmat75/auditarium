// SPDX-License-Identifier: MIT
namespace Auditarium.Dal.Configuration;
public sealed class DatabaseOptions { public const string SectionName = "Auditarium:Database"; public required string Provider { get; init; } public required string ConnectionString { get; init; } public int BootstrapTimeoutSeconds { get; init; } = 180; }
public sealed class RecoveryOptions { public const string SectionName = "Auditarium:Recovery"; public bool Enabled { get; init; } public string? DefaultAdminPassword { get; init; } }
