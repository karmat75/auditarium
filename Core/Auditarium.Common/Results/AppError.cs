// SPDX-License-Identifier: MIT
namespace Auditarium.Common.Results;

public sealed record AppError(
    string Code,
    ErrorType Type,
    string? Target = null,
    IReadOnlyDictionary<string, object?>? Parameters = null);
