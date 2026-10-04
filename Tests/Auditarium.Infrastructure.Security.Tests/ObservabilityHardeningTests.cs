// SPDX-License-Identifier: MIT
using System.Text;
using Auditarium.Api;
using Auditarium.Bll.Pipeline;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Auditarium.Infrastructure.Security.Tests;

public sealed class ObservabilityHardeningTests
{
    [Fact]
    public async Task Unexpected_api_errors_expose_only_the_safe_problem_contract()
    {
        const string sensitiveExceptionText = "credential=do-not-disclose";
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var handler = new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance);

        Assert.True(await handler.TryHandleAsync(context, new InvalidOperationException(sensitiveExceptionText), CancellationToken.None));

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains("SYSTEM.UNEXPECTED_ERROR", body, StringComparison.Ordinal);
        Assert.Contains("traceId", body, StringComparison.Ordinal);
        Assert.DoesNotContain(sensitiveExceptionText, body, StringComparison.Ordinal);
        Assert.DoesNotContain("stackTrace", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Cqrs_observability_does_not_log_request_payloads()
    {
        const string sensitivePayload = "aud_v1_key_do-not-log";
        var logger = new RecordingLogger<ObservabilityBehavior<SensitiveProbeRequest, Result>>();
        var behavior = new ObservabilityBehavior<SensitiveProbeRequest, Result>(logger);

        var result = await behavior.Handle(
            new SensitiveProbeRequest(sensitivePayload),
            (_, _) => ValueTask.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(logger.Messages);
        Assert.DoesNotContain(logger.Messages, message => message.Contains(sensitivePayload, StringComparison.Ordinal));
    }

    [AllowAnonymous]
    private sealed record SensitiveProbeRequest(string Credential) : IRequest<Result>;

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Messages.Add(formatter(state, exception));
    }
}
