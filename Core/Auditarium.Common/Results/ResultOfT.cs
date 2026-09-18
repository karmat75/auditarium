// SPDX-License-Identifier: MIT
namespace Auditarium.Common.Results;

public sealed class Result<T> : IAppResult
{
    private Result(T? value, IReadOnlyList<AppError> errors)
    {
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Errors.Count == 0;
    public T? Value { get; }
    public IReadOnlyList<AppError> Errors { get; }

    public static Result<T> Success(T value) => new(value, []);
    public static Result<T> Failure(params AppError[] errors) => new(default, errors);
}
