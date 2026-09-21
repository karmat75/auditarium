// SPDX-License-Identifier: MIT
using System.Text.RegularExpressions;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Abstractions.Security;
using Auditarium.Bll.Abstractions.Settings;
using Auditarium.Bll.Features.Administration.Settings;
using Auditarium.Bll.Security;
using Auditarium.Bll.Settings;
using Auditarium.Common.Results;
using Auditarium.Models.Identity;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Identity.AuthenticationProviders;

public sealed record AuthenticationProviderListItem(long ProviderId, string ProviderKey, string ProviderType, string DisplayName, bool IsEnabled, bool SystemManaged, long ConcurrencyVersion);
public sealed record AuthenticationProviderSetting(
    string Property, string DisplayName, string DataType, string? ConfiguredValue, string? EffectiveValue,
    SettingValueSource Source, bool IsExternallyOverridden, bool Secret, bool IsConfigured,
    bool IsEffectivelyConfigured, IReadOnlyList<string> AllowedValues, long ConcurrencyVersion);
public sealed record AuthenticationProviderDetails(
    long ProviderId, string ProviderKey, string ProviderType, string DisplayName, bool IsEnabled,
    bool SystemManaged, IReadOnlyList<AuthenticationProviderSetting> Settings, long ConcurrencyVersion);

[RequiresPermission("Authentication.Manage")] public sealed record ListAuthenticationProvidersQuery() : IRequest<Result<IReadOnlyList<AuthenticationProviderListItem>>>;
[RequiresPermission("Authentication.Manage")] public sealed record GetAuthenticationProviderQuery(long ProviderId) : IRequest<Result<AuthenticationProviderDetails>>;
[RequiresPermission("Authentication.Manage")] public sealed record CreateLdapProviderCommand(string ProviderKey, string DisplayName) : IRequest<Result<long>>;
[RequiresPermission("Authentication.Manage")] public sealed record UpdateLdapProviderCommand(long ProviderId, string DisplayName, IReadOnlyDictionary<string, string?> Values, IReadOnlyDictionary<string, long> SettingVersions, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Authentication.Manage")] public sealed record ResetLdapProviderSettingCommand(long ProviderId, string Property, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Authentication.Manage")] public sealed record SetAuthenticationProviderEnabledCommand(long ProviderId, bool IsEnabled, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Authentication.Manage")] public sealed record DeleteAuthenticationProviderCommand(long ProviderId, long ConcurrencyVersion) : IRequest<Result>;
[RequiresPermission("Authentication.Manage")] public sealed record TestLdapProviderConnectionCommand(long ProviderId) : IRequest<Result>;

public sealed class AuthenticationProviderCommandHandler(
    IAuditariumDbContext db,
    ILdapAuthenticationService ldapAuthentication,
    IApplicationSettingResolver resolver,
    ISecretProtector secretProtector) :
    IRequestHandler<ListAuthenticationProvidersQuery, Result<IReadOnlyList<AuthenticationProviderListItem>>>,
    IRequestHandler<GetAuthenticationProviderQuery, Result<AuthenticationProviderDetails>>,
    IRequestHandler<CreateLdapProviderCommand, Result<long>>,
    IRequestHandler<UpdateLdapProviderCommand, Result>,
    IRequestHandler<ResetLdapProviderSettingCommand, Result>,
    IRequestHandler<SetAuthenticationProviderEnabledCommand, Result>,
    IRequestHandler<DeleteAuthenticationProviderCommand, Result>,
    IRequestHandler<TestLdapProviderConnectionCommand, Result>
{
    public async ValueTask<Result<IReadOnlyList<AuthenticationProviderListItem>>> Handle(ListAuthenticationProvidersQuery query, CancellationToken ct)
    {
        var items = await db.AuthenticationProviders.AsNoTracking().OrderBy(x => x.ProviderType).ThenBy(x => x.ProviderKey)
            .Select(x => new AuthenticationProviderListItem(x.AuthenticationProviderId, x.ProviderKey, x.ProviderType, x.DisplayName, x.IsEnabled,
                x.ProviderKey == "LOCAL" || x.ProviderKey == "API", x.ConcurrencyVersion)).ToListAsync(ct);
        return Result<IReadOnlyList<AuthenticationProviderListItem>>.Success(items);
    }

    public async ValueTask<Result<AuthenticationProviderDetails>> Handle(GetAuthenticationProviderQuery query, CancellationToken ct)
    {
        var provider = await db.AuthenticationProviders.AsNoTracking().SingleOrDefaultAsync(x => x.AuthenticationProviderId == query.ProviderId, ct);
        if (provider is null) return Failure<AuthenticationProviderDetails>("AUTH_PROVIDER.NOT_FOUND", ErrorType.NotFound);
        var settings = new List<AuthenticationProviderSetting>();
        if (provider.ProviderType == "LDAP")
        {
            var definitions = LdapProviderSettingDefinitions.For(provider.ProviderKey);
            var prefix = $"Authentication:Providers:{provider.ProviderKey}:";
            var rows = await db.ApplicationSettings.AsNoTracking().Where(x => x.SettingKey.StartsWith(prefix)).ToDictionaryAsync(x => x.SettingKey, ct);
            foreach (var definition in definitions.OrderBy(x => x.DisplayOrder))
            {
                var effective = await resolver.GetAsync(definition, ct);
                rows.TryGetValue(definition.Key, out var row);
                var configured = row is null ? null : SettingAdministrationHandler.ReadStored(row.SerializedValue, definition);
                settings.Add(new AuthenticationProviderSetting(
                    Property(definition.Key), definition.DisplayName ?? definition.Key, definition.DataType,
                    definition.Secret ? null : configured, definition.Secret ? null : effective.Value, effective.Source,
                    effective.IsExternallyOverridden, definition.Secret, definition.Secret ? !string.IsNullOrEmpty(configured) : row is not null,
                    !string.IsNullOrEmpty(effective.Value), definition.AllowedValues ?? [], row?.ConcurrencyVersion ?? 0));
            }
        }
        return Result<AuthenticationProviderDetails>.Success(new(provider.AuthenticationProviderId, provider.ProviderKey, provider.ProviderType,
            provider.DisplayName, provider.IsEnabled, provider.ProviderKey is "LOCAL" or "API", settings, provider.ConcurrencyVersion));
    }

    public async ValueTask<Result<long>> Handle(CreateLdapProviderCommand message, CancellationToken ct)
    {
        var key = message.ProviderKey.Trim().ToUpperInvariant();
        if (!Regex.IsMatch(key, "^[A-Z][A-Z0-9_]{1,62}$") || key is "LOCAL" or "API") return Failure<long>("AUTH_PROVIDER.INVALID_KEY");
        if (string.IsNullOrWhiteSpace(message.DisplayName)) return Failure<long>("AUTH_PROVIDER.DISPLAY_NAME_REQUIRED");
        if (await db.AuthenticationProviders.AnyAsync(x => x.ProviderKey == key, ct)) return Failure<long>("AUTH_PROVIDER.KEY_EXISTS", ErrorType.Conflict);
        var provider = new AuthenticationProvider { ProviderKey = key, ProviderType = "LDAP", DisplayName = message.DisplayName.Trim(), IsEnabled = false };
        db.AuthenticationProviders.Add(provider); await db.SaveChangesAsync(ct);
        return Result<long>.Success(provider.AuthenticationProviderId);
    }

    public async ValueTask<Result> Handle(UpdateLdapProviderCommand message, CancellationToken ct)
    {
        var provider = await EditableLdapAsync(message.ProviderId, ct); if (!provider.IsSuccess) return Result.Failure(provider.Errors.ToArray());
        var entity = provider.Value!;
        if (entity.ConcurrencyVersion != message.ConcurrencyVersion) return Failure("AUTH_PROVIDER.CONCURRENCY_CONFLICT", ErrorType.Conflict);
        if (string.IsNullOrWhiteSpace(message.DisplayName)) return Failure("AUTH_PROVIDER.DISPLAY_NAME_REQUIRED");
        var definitions = LdapProviderSettingDefinitions.For(entity.ProviderKey).ToDictionary(x => Property(x.Key), StringComparer.Ordinal);
        foreach (var pair in message.Values)
        {
            if (!definitions.TryGetValue(pair.Key, out var definition)) return Failure("AUTH_PROVIDER.SETTING_UNKNOWN");
            if (definition.Secret && string.IsNullOrEmpty(pair.Value)) continue;
            var value = pair.Value ?? string.Empty;
            if (!definition.IsValid(value)) return Failure("AUTH_PROVIDER.SETTING_INVALID");
            var effective = await resolver.GetAsync(definition, ct);
            if (effective.IsExternallyOverridden) return Failure("AUTH_PROVIDER.SETTING_EXTERNALLY_OVERRIDDEN", ErrorType.Conflict);
            var row = await db.ApplicationSettings.SingleOrDefaultAsync(x => x.SettingKey == definition.Key, ct);
            var expectedVersion = message.SettingVersions.GetValueOrDefault(pair.Key);
            if (row is null && expectedVersion != 0 || row is not null && row.ConcurrencyVersion != expectedVersion) return Failure("AUTH_PROVIDER.SETTING_CONCURRENCY_CONFLICT", ErrorType.Conflict);
            if (row is null) { row = new ApplicationSetting { SettingKey = definition.Key, SerializedValue = string.Empty }; db.ApplicationSettings.Add(row); }
            var storedValue = definition.Secret ? secretProtector.Protect(value, definition.Key) : value;
            row.SerializedValue = SettingAdministrationHandler.Serialize(definition.DataType, storedValue);
        }
        entity.DisplayName = message.DisplayName.Trim();
        return await SaveAsync("AUTH_PROVIDER.CONCURRENCY_CONFLICT", ct);
    }

    public async ValueTask<Result> Handle(ResetLdapProviderSettingCommand message, CancellationToken ct)
    {
        var provider = await EditableLdapAsync(message.ProviderId, ct); if (!provider.IsSuccess) return Result.Failure(provider.Errors.ToArray());
        var definition = LdapProviderSettingDefinitions.For(provider.Value!.ProviderKey).SingleOrDefault(x => Property(x.Key) == message.Property);
        if (definition is null) return Failure("AUTH_PROVIDER.SETTING_UNKNOWN", ErrorType.NotFound);
        var row = await db.ApplicationSettings.SingleOrDefaultAsync(x => x.SettingKey == definition.Key, ct);
        if (row is null) return Result.Success();
        if (row.ConcurrencyVersion != message.ConcurrencyVersion) return Failure("AUTH_PROVIDER.SETTING_CONCURRENCY_CONFLICT", ErrorType.Conflict);
        db.ApplicationSettings.Remove(row); await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result> Handle(SetAuthenticationProviderEnabledCommand message, CancellationToken ct)
    {
        var provider = await db.AuthenticationProviders.SingleOrDefaultAsync(x => x.AuthenticationProviderId == message.ProviderId, ct);
        if (provider is null) return Failure("AUTH_PROVIDER.NOT_FOUND", ErrorType.NotFound);
        if (provider.ProviderKey is "LOCAL" or "API") return Failure("AUTH_PROVIDER.SYSTEM_MANAGED", ErrorType.Forbidden);
        if (provider.ConcurrencyVersion != message.ConcurrencyVersion) return Failure("AUTH_PROVIDER.CONCURRENCY_CONFLICT", ErrorType.Conflict);
        provider.IsEnabled = message.IsEnabled; return await SaveAsync("AUTH_PROVIDER.CONCURRENCY_CONFLICT", ct);
    }

    public async ValueTask<Result> Handle(DeleteAuthenticationProviderCommand message, CancellationToken ct)
    {
        var provider = await db.AuthenticationProviders.SingleOrDefaultAsync(x => x.AuthenticationProviderId == message.ProviderId, ct);
        if (provider is null) return Failure("AUTH_PROVIDER.NOT_FOUND", ErrorType.NotFound);
        if (provider.ProviderKey is "LOCAL" or "API") return Failure("AUTH_PROVIDER.SYSTEM_MANAGED", ErrorType.Forbidden);
        if (provider.ConcurrencyVersion != message.ConcurrencyVersion) return Failure("AUTH_PROVIDER.CONCURRENCY_CONFLICT", ErrorType.Conflict);
        if (await db.UserIdentities.AnyAsync(x => x.AuthenticationProviderId == provider.AuthenticationProviderId, ct)) return Failure("AUTH_PROVIDER.HAS_DEPENDENCIES", ErrorType.Conflict);
        var prefix = $"Authentication:Providers:{provider.ProviderKey}:";
        db.ApplicationSettings.RemoveRange(await db.ApplicationSettings.Where(x => x.SettingKey.StartsWith(prefix)).ToListAsync(ct));
        db.AuthenticationProviders.Remove(provider); await db.SaveChangesAsync(ct); return Result.Success();
    }

    public async ValueTask<Result> Handle(TestLdapProviderConnectionCommand message, CancellationToken ct)
    {
        var provider = await db.AuthenticationProviders.AsNoTracking().SingleOrDefaultAsync(x => x.AuthenticationProviderId == message.ProviderId && x.ProviderType == "LDAP", ct);
        if (provider is null) return Failure("AUTH_PROVIDER.NOT_FOUND", ErrorType.NotFound);
        try { await ldapAuthentication.ValidateConnectionAsync(provider.ProviderKey, ct); return Result.Success(); }
        catch (Exception exception) when (exception is not OperationCanceledException) { return Failure("LDAP.CONNECTION_TEST_FAILED"); }
    }

    private async Task<Result<AuthenticationProvider>> EditableLdapAsync(long providerId, CancellationToken ct)
    {
        var provider = await db.AuthenticationProviders.SingleOrDefaultAsync(x => x.AuthenticationProviderId == providerId, ct);
        if (provider is null) return Failure<AuthenticationProvider>("AUTH_PROVIDER.NOT_FOUND", ErrorType.NotFound);
        return provider.ProviderType == "LDAP" && provider.ProviderKey is not ("LOCAL" or "API")
            ? Result<AuthenticationProvider>.Success(provider)
            : Failure<AuthenticationProvider>("AUTH_PROVIDER.SYSTEM_MANAGED", ErrorType.Forbidden);
    }
    private async Task<Result> SaveAsync(string code, CancellationToken ct) { try { await db.SaveChangesAsync(ct); return Result.Success(); } catch (DbUpdateConcurrencyException) { return Failure(code, ErrorType.Conflict); } }
    private static string Property(string key) => key[(key.LastIndexOf(':') + 1)..];
    private static Result Failure(string code, ErrorType type = ErrorType.Validation) => Result.Failure(new AppError(code, type));
    private static Result<T> Failure<T>(string code, ErrorType type = ErrorType.Validation) => Result<T>.Failure(new AppError(code, type));
}
