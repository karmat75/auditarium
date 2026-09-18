// SPDX-License-Identifier: MIT
namespace Auditarium.Common.Results;

public sealed class Result : IAppResult
{
    private Result(IReadOnlyList<AppError> errors) => Errors = errors;

    public bool IsSuccess => Errors.Count == 0;
    public IReadOnlyList<AppError> Errors { get; }

    public static Result Success() => new([]);
    public static Result Failure(params AppError[] errors) => new(errors);
}
