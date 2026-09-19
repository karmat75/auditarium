// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Settings;
public sealed record SettingDefinition(string Key, string DataType, string DefaultJson, bool UiEditable, Func<string, bool> IsValid);
public static class SettingDefinitions
{
    public static IReadOnlyList<SettingDefinition> All { get; } =
    [
        new("Security:LocalPassword:MinimumLength", "int", "15", true, v => int.TryParse(v, out var n) && n is >= 12 and <= 128),
        new("Security:LocalPassword:MaximumLength", "int", "128", true, v => int.TryParse(v, out var n) && n is >= 12 and <= 1024),
        new("Security:LocalLockout:FailedAttempts", "int", "5", true, v => int.TryParse(v, out var n) && n is >= 1 and <= 20),
        new("Security:LocalLockout:WindowMinutes", "int", "15", true, v => int.TryParse(v, out var n) && n is >= 1 and <= 1440),
        new("Security:LocalLockout:DurationMinutes", "int", "15", true, v => int.TryParse(v, out var n) && n is >= 1 and <= 1440),
        new("Security:ApiCredentials:DefaultLifetimeDays", "int", "180", true, v => int.TryParse(v, out var n) && n is >= 1 and <= 3650),
        new("Security:ApiCredentials:MaximumActiveCredentials", "int", "5", true, v => int.TryParse(v, out var n) && n is >= 1 and <= 50)
    ];
}
