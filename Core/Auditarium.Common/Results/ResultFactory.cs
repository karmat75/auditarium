// SPDX-License-Identifier: MIT
namespace Auditarium.Common.Results;

public static class ResultFactory
{
    public static TResponse Failure<TResponse>(params AppError[] errors)
        where TResponse : IAppResult
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(errors);
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResponse).GetGenericArguments()[0];
            var method = typeof(Result<>).MakeGenericType(valueType).GetMethod(nameof(Result<object>.Failure));
            return (TResponse)method!.Invoke(null, [errors])!;
        }

        throw new InvalidOperationException($"Unsupported result type {typeof(TResponse).FullName}.");
    }
}
