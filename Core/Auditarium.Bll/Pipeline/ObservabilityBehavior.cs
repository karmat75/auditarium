// SPDX-License-Identifier: MIT
using System.Diagnostics;
using Auditarium.Bll.Observability;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Auditarium.Bll.Pipeline;

public sealed class ObservabilityBehavior<TMessage, TResponse>(ILogger<ObservabilityBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TMessage).Name;
        var stopwatch = Stopwatch.StartNew();
        using var activity = AuditariumTelemetry.ActivitySource.StartActivity($"CQRS {requestType}");
        logger.LogInformation("CQRS request {RequestType} started", requestType);
        AuditariumTelemetry.CqrsRequests.Add(1, new KeyValuePair<string, object?>("request_type", requestType));

        try
        {
            var response = await next(message, cancellationToken);
            logger.LogInformation("CQRS request {RequestType} completed in {DurationMs} ms", requestType, stopwatch.Elapsed.TotalMilliseconds);
            return response;
        }
        catch (Exception exception)
        {
            AuditariumTelemetry.CqrsErrors.Add(1, new KeyValuePair<string, object?>("request_type", requestType));
            activity?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(exception, "CQRS request {RequestType} failed", requestType);
            throw;
        }
        finally
        {
            AuditariumTelemetry.CqrsRequestDuration.Record(stopwatch.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("request_type", requestType));
        }
    }
}
