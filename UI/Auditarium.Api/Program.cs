// SPDX-License-Identifier: MIT
using System.Diagnostics;
using Auditarium.Bll;
using Auditarium.Dal;
using Auditarium.Infrastructure.Security;
using Auditarium.Infrastructure.Ldap;
using Auditarium.Bll.Features.System.GetHostStatus;
using Auditarium.Common.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuditariumBll();
builder.Services.AddAuditariumPersistence(builder.Configuration);
builder.Services.AddAuditariumDataProtection(builder.Configuration);
builder.Services.AddAuditariumLdap();
builder.Services.AddAuditariumApiAuthentication();
builder.Services.AddAuditariumHttpCurrentActor();
builder.Services.AddHealthChecks()
    .AddCheck("startup", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["ready"]);
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
var otlpEndpointText = builder.Configuration["OpenTelemetry:Otlp:Endpoint"];
var otlpEnabled = builder.Configuration.GetValue<bool>("OpenTelemetry:Enabled") &&
    Uri.TryCreate(otlpEndpointText, UriKind.Absolute, out _);
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Configuration["OpenTelemetry:ServiceName"] ?? "Auditarium"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation().AddSource("Auditarium");
        if (otlpEnabled) tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpointText!, UriKind.Absolute));
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation().AddRuntimeInstrumentation().AddMeter("Auditarium").AddPrometheusExporter();
        if (otlpEnabled) metrics.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpointText!, UriKind.Absolute));
    });

var app = builder.Build();
await app.Services.InitializeAuditariumDatabaseAsync();
var recoveryMode = builder.Configuration.GetValue<bool>("Auditarium:Recovery:Enabled");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = registration => registration.Tags.Contains("ready") });
app.MapPrometheusScrapingEndpoint("/metrics");
if (recoveryMode)
{
    app.MapGet("/recovery", () => Results.Ok(new { status = "recovery", message = "Auditarium befindet sich im Recovery-Modus. Normalbetrieb ist gesperrt." }));
    app.Run();
}
app.MapGet("/api/v1/system/status", async (Mediator.IMediator mediator, CancellationToken cancellationToken) =>
{
    var result = await mediator.Send(new GetHostStatusQuery(), cancellationToken);
    return result.ToHttpResult();
})
    .WithName("GetSystemStatus")
    .WithSummary("Returns the API host status.")
    .Produces<HostStatusViewModel>(StatusCodes.Status200OK)
    .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

app.Run();

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception for {RequestPath}; trace {TraceId}", context.Request.Path, Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier);
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Extensions = { ["code"] = "SYSTEM.UNEXPECTED_ERROR", ["traceId"] = traceId }
        }, cancellationToken);
        return true;
    }
}

public static class ResultHttpMapping
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var firstError = result.Errors[0];
        var status = firstError.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
        return Results.Problem(statusCode: status, title: firstError.Type.ToString(), extensions: new Dictionary<string, object?>
        {
            ["code"] = firstError.Code,
            ["errors"] = result.Errors,
            ["traceId"] = Activity.Current?.TraceId.ToString()
        });
    }
}
