// SPDX-License-Identifier: MIT
using System.Diagnostics;
using Auditarium.Common.Results;
using Microsoft.AspNetCore.Http;

namespace Auditarium.Api;

public static class ApiProblemDetails
{
    public static IResult From<T>(Result<T> result) => result.IsSuccess
        ? Results.Ok(result.Value)
        : FromErrors(result.Errors);

    public static IResult From(Result result) => result.IsSuccess
        ? Results.NoContent()
        : FromErrors(result.Errors);

    public static IResult FromErrors(IReadOnlyList<AppError> errors)
    {
        var firstError = errors[0];
        var status = StatusFor(firstError.Type);
        return Results.Problem(
            statusCode: status,
            title: firstError.Type.ToString(),
            extensions: new Dictionary<string, object?>
            {
                ["code"] = firstError.Code,
                ["errors"] = errors,
                ["traceId"] = TraceId()
            });
    }

    public static int StatusFor(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    public static string TraceId() => Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
}
