// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Settings;
using Auditarium.Bll.Settings;
using Auditarium.Bll.Abstractions.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Auditarium.Dal.Settings;

internal sealed class ApplicationSettingResolver(AuditariumDbContext db, IConfiguration configuration, ISecretProtector secretProtector) : IApplicationSettingResolver
{
    public async Task<EffectiveSetting> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var definition = SettingDefinitions.All.SingleOrDefault(x => x.Key == key) ?? throw new KeyNotFoundException($"Unknown setting key '{key}'.");
        return await GetAsync(definition, cancellationToken);
    }

    public async Task<EffectiveSetting> GetAsync(SettingDefinition definition, CancellationToken cancellationToken = default)
    {
        var key = definition.Key;
        var environment = Environment.GetEnvironmentVariable("AUDITARIUM__" + key.Replace(":", "__"));
        if (environment is not null && definition.IsValid(environment)) return new EffectiveSetting(key, environment, SettingValueSource.Environment, true);
        if (definition.UiEditable)
        {
            var stored = await db.ApplicationSettings.AsNoTracking().SingleOrDefaultAsync(x => x.SettingKey == key, cancellationToken);
            if (stored is not null && TryRead(stored.SerializedValue, definition, out var value))
            {
                if (definition.Secret && !string.IsNullOrEmpty(value)) value = secretProtector.Unprotect(value, key);
                return new EffectiveSetting(key, value, SettingValueSource.Database, false);
            }
        }
        var configured = configuration["Auditarium:" + key];
        if (configured is not null && definition.IsValid(configured)) return new EffectiveSetting(key, configured, SettingValueSource.Configuration, false);
        return new EffectiveSetting(key, definition.DefaultValue, SettingValueSource.Default, false);
    }

    private static bool TryRead(string serialized, SettingDefinition definition, out string value)
    {
        value = string.Empty;
        try
        {
            using var document = JsonDocument.Parse(serialized);
            if (document.RootElement.GetProperty("datatype").GetString() != definition.DataType) return false;
            value = document.RootElement.GetProperty("value").ToString();
            return definition.IsValid(value);
        }
        catch (JsonException) { return false; }
        catch (KeyNotFoundException) { return false; }
    }
}
