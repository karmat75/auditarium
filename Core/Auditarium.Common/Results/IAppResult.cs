// SPDX-License-Identifier: MIT
namespace Auditarium.Common.Results;

public interface IAppResult
{
    bool IsSuccess { get; }
    IReadOnlyList<AppError> Errors { get; }
}
