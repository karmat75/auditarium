// SPDX-License-Identifier: MIT
using Auditarium.Common.Results;

namespace Auditarium.Api;

public sealed record PageResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount);

public sealed record PageRequest(int Page = 1, int PageSize = 50)
{
    public const int MaximumPageSize = 200;

    public Result<PageRequest> Validate() => Page < 1
        ? Result<PageRequest>.Failure(new AppError("API.PAGINATION.PAGE_INVALID", ErrorType.Validation, "page"))
        : PageSize is < 1 or > MaximumPageSize
            ? Result<PageRequest>.Failure(new AppError("API.PAGINATION.PAGE_SIZE_INVALID", ErrorType.Validation, "pageSize"))
            : Result<PageRequest>.Success(this);
}

public enum SortDirection
{
    Ascending,
    Descending
}

public sealed record SortRequest(string Field, SortDirection Direction)
{
    public static Result<SortRequest?> Parse(string? sort, IReadOnlySet<string> allowedFields)
    {
        if (string.IsNullOrWhiteSpace(sort)) return Result<SortRequest?>.Success(null);

        var descending = sort[0] == '-';
        var field = descending ? sort[1..] : sort;
        return string.IsNullOrWhiteSpace(field) || !allowedFields.Contains(field)
            ? Result<SortRequest?>.Failure(new AppError("API.SORT.FIELD_NOT_ALLOWED", ErrorType.Validation, "sort"))
            : Result<SortRequest?>.Success(new SortRequest(field, descending ? SortDirection.Descending : SortDirection.Ascending));
    }
}
