// SPDX-License-Identifier: MIT
using Auditarium.Bll;
using Auditarium.Dal;
using Auditarium.Infrastructure.Security;
using Auditarium.Infrastructure.Ldap;
using Auditarium.Fal;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuditariumBll();
builder.Services.AddAuditariumPersistence(builder.Configuration);
builder.Services.AddAuditariumJobScheduling();
builder.Services.AddAuditariumFileStorage(builder.Configuration);
builder.Services.AddAuditariumDataProtection(builder.Configuration);
builder.Services.AddAuditariumLdap();
builder.Services.AddAuditariumCookieAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddAuditariumHttpCurrentActor();
builder.Services.AddRazorPages(options =>
    options.Conventions.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddAntiforgery();
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
await app.Services.InitializeAuditariumDatabaseAsync();
var recoveryMode = builder.Configuration.GetValue<bool>("Auditarium:Recovery:Enabled");
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
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    var mustChangePassword = context.User.HasClaim(AuditariumAuthenticationSchemes.MustChangePasswordClaim, "true");
    var path = context.Request.Path;
    if (mustChangePassword && !path.StartsWithSegments("/account/change-password") && !path.StartsWithSegments("/logout"))
    {
        context.Response.Redirect("/account/change-password");
        return;
    }

    await next(context);
});
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = registration => registration.Tags.Contains("ready") });
app.MapPrometheusScrapingEndpoint("/metrics");
if (recoveryMode)
{
    app.MapGet("/recovery", () => Results.Content("<main><h1>Auditarium Recovery-Modus</h1><p>Der Normalbetrieb ist gesperrt. Beenden Sie diese einzelne Recovery-Instanz nach Abschluss und entfernen Sie die Recovery-Konfiguration.</p></main>", "text/html; charset=utf-8"));
    app.Run();
}
app.MapRazorPages();
app.Run();
