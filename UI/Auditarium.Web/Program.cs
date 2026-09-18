// SPDX-License-Identifier: MIT
using Auditarium.Bll;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuditariumBll();
builder.Services.AddAuditariumAnonymousActor();
builder.Services.AddRazorPages();
builder.Services.AddHealthChecks()
    .AddCheck("startup", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["ready"]);
builder.Services.AddProblemDetails();
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
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = registration => registration.Tags.Contains("ready") });
app.MapPrometheusScrapingEndpoint("/metrics");
app.MapRazorPages();
app.Run();
