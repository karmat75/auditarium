// SPDX-License-Identifier: MIT
using System.Security.Claims;
using System.Text.Encodings.Web;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auditarium.Infrastructure.Security;

public static class AuditariumAuthenticationSchemes
{
    public const string Cookie = "AuditariumCookie";
    public const string ApiBearer = "AuditariumApiBearer";
    public const string MustChangePasswordClaim = "auditarium:must_change_password";
}

public static class HttpAuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddAuditariumCookieAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(AuditariumAuthenticationSchemes.Cookie).AddCookie(AuditariumAuthenticationSchemes.Cookie, options =>
        {
            options.LoginPath = "/login";
            options.AccessDeniedPath = "/access-denied";
            options.Cookie.Name = "__Host-Auditarium";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });
        return services;
    }

    public static IServiceCollection AddAuditariumApiAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(AuditariumAuthenticationSchemes.ApiBearer).AddScheme<AuthenticationSchemeOptions, ApiCredentialAuthenticationHandler>(AuditariumAuthenticationSchemes.ApiBearer, _ => { });
        return services;
    }

    public static IServiceCollection AddAuditariumHttpCurrentActor(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActor, HttpCurrentActor>();
        return services;
    }
}

public sealed class HttpCurrentActor(IHttpContextAccessor accessor) : ICurrentActor
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;
    public ActorType Type => IsAuthenticated ? ActorType.User : ActorType.Anonymous;
    public long? UserId => long.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) && userId > 0 ? userId : null;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true && UserId is not null;
    public bool MustChangePassword => string.Equals(Principal?.FindFirstValue(AuditariumAuthenticationSchemes.MustChangePasswordClaim), "true", StringComparison.Ordinal);
}

public sealed class ApiCredentialAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IApiCredentialService credentials,
    IAuditEventWriter auditEvents)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(header)) return AuthenticateResult.NoResult();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await RecordFailedLoginAsync();
            return AuthenticateResult.Fail("Unsupported authentication scheme.");
        }
        var userId = await credentials.AuthenticateAsync(header[7..].Trim(), Context.RequestAborted);
        if (userId is null) { await RecordFailedLoginAsync(); return AuthenticateResult.Fail("Invalid API credential."); }
        await auditEvents.WriteAsync(new AuditEvent("LOGIN", "User", userId, userId), Context.RequestAborted);
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))], Scheme.Name);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }

    private async Task RecordFailedLoginAsync()
    {
        try { await auditEvents.WriteAsync(new AuditEvent("LOGIN_FAILED", "Authentication"), Context.RequestAborted); }
        catch (Exception exception) { Logger.LogError(exception, "Audit log write failed while API authentication remained rejected."); }
    }
}
