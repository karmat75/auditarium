// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Abstractions.Settings;

using Auditarium.Bll.Settings;

public enum SettingValueSource { Default, Configuration, Database, Environment }
public sealed record EffectiveSetting(string Key, string Value, SettingValueSource Source, bool IsExternallyOverridden);
public interface IApplicationSettingResolver
{
    Task<EffectiveSetting> GetAsync(string key, CancellationToken cancellationToken = default);
    Task<EffectiveSetting> GetAsync(SettingDefinition definition, CancellationToken cancellationToken = default);
}
