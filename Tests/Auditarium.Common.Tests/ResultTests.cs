// SPDX-License-Identifier: MIT
using Auditarium.Common.Results;
using Xunit;

namespace Auditarium.Common.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Failure_retains_the_stable_error_contract()
    {
        var error = new AppError("SYSTEM.TEST_FAILURE", ErrorType.Conflict, "value");

        var result = Result<string>.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.Equal("SYSTEM.TEST_FAILURE", Assert.Single(result.Errors).Code);
        Assert.Equal(ErrorType.Conflict, result.Errors[0].Type);
    }
}
