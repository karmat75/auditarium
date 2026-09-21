// SPDX-License-Identifier: MIT
using System.Globalization;
using System.Text.Json;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Abstractions.Security;
using Auditarium.Bll.Abstractions.Settings;
using Auditarium.Bll.Security;
using Auditarium.Bll.Settings;
using Auditarium.Common.Results;
using Auditarium.Models.Identity;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Administration.Settings;

public sealed record SettingDetails(
    string Key, string DisplayName, string Category, string DataType, string? ConfiguredValue,
    string? EffectiveValue, SettingValueSource Source, bool UiEditable, bool IsExternallyOverridden,
    bool Secret, bool IsConfigured, bool IsEffectivelyConfigured, bool RestartRequired, IReadOnlyList<string> AllowedValues,
    long ConcurrencyVersion);

[RequiresPermission("Settings.Manage")]
public sealed record ListSettingsQuery() : IRequest<Result<IReadOnlyList<SettingDetails>>>;
[RequiresPermission("Settings.Manage")]
public sealed record UpdateSettingCommand(string Key, string Value, long ConcurrencyVersion) : IRequest<Result>;

public sealed class SettingAdministrationHandler(IAuditariumDbContext db, IApplicationSettingResolver resolver, ISecretProtector secretProtector) :
    IRequestHandler<ListSettingsQuery, Result<IReadOnlyList<SettingDetails>>>, IRequestHandler<UpdateSettingCommand, Result>
{
    public async ValueTask<Result<IReadOnlyList<SettingDetails>>> Handle(ListSettingsQuery query, CancellationToken ct)
    {
        var stored = await db.ApplicationSettings.AsNoTracking().ToDictionaryAsync(x => x.SettingKey, ct);
        var result = new List<SettingDetails>();
        foreach (var definition in SettingDefinitions.All.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Key, StringComparer.Ordinal))
        {
            var effective = await resolver.GetAsync(definition, ct);
            stored.TryGetValue(definition.Key, out var configured);
            var configuredValue = configured is null ? null : ReadStored(configured.SerializedValue, definition);
            var isConfigured = definition.Secret ? !string.IsNullOrEmpty(configuredValue) : configured is not null;
            result.Add(new SettingDetails(
                definition.Key, definition.DisplayName ?? definition.Key, definition.Category, definition.DataType,
                definition.Secret ? null : configuredValue, definition.Secret ? null : effective.Value, effective.Source,
                definition.UiEditable, effective.IsExternallyOverridden, definition.Secret, isConfigured, !string.IsNullOrEmpty(effective.Value),
                definition.RestartRequired, definition.AllowedValues ?? [], configured?.ConcurrencyVersion ?? 0));
        }
        return Result<IReadOnlyList<SettingDetails>>.Success(result);
    }

    public async ValueTask<Result> Handle(UpdateSettingCommand command, CancellationToken ct)
    {
        var definition = SettingDefinitions.All.SingleOrDefault(x => x.Key == command.Key);
        if (definition is null) return Failure("SETTING.UNKNOWN", ErrorType.NotFound);
        if (!definition.UiEditable) return Failure("SETTING.READ_ONLY", ErrorType.Forbidden);
        var effective = await resolver.GetAsync(definition, ct);
        if (effective.IsExternallyOverridden) return Failure("SETTING.EXTERNALLY_OVERRIDDEN", ErrorType.Conflict);
        if (!definition.IsValid(command.Value) || definition.Secret && string.IsNullOrEmpty(command.Value)) return Failure("SETTING.VALUE_INVALID");
        var setting = await db.ApplicationSettings.SingleOrDefaultAsync(x => x.SettingKey == command.Key, ct);
        if (setting is null)
        {
            if (command.ConcurrencyVersion != 0) return Failure("SETTING.CONCURRENCY_CONFLICT", ErrorType.Conflict);
            setting = new ApplicationSetting { SettingKey = command.Key, SerializedValue = string.Empty };
            db.ApplicationSettings.Add(setting);
        }
        else if (setting.ConcurrencyVersion != command.ConcurrencyVersion) return Failure("SETTING.CONCURRENCY_CONFLICT", ErrorType.Conflict);
        var value = definition.Secret ? secretProtector.Protect(command.Value, definition.Key) : command.Value;
        setting.SerializedValue = Serialize(definition.DataType, value);
        try { await db.SaveChangesAsync(ct); return Result.Success(); }
        catch (DbUpdateConcurrencyException) { return Failure("SETTING.CONCURRENCY_CONFLICT", ErrorType.Conflict); }
    }

    public static string Serialize(string dataType, string value)
    {
        object? typed = dataType switch
        {
            "int" => int.Parse(value, CultureInfo.InvariantCulture),
            "float" => double.Parse(value, CultureInfo.InvariantCulture),
            "bool" => bool.Parse(value),
            _ => value
        };
        return JsonSerializer.Serialize(new { datatype = dataType, value = typed });
    }

    public static string? ReadStored(string serialized, SettingDefinition definition)
    {
        try
        {
            using var document = JsonDocument.Parse(serialized);
            if (document.RootElement.GetProperty("datatype").GetString() != definition.DataType) return null;
            var value = document.RootElement.GetProperty("value");
            return value.ValueKind == JsonValueKind.Null ? null : value.ToString();
        }
        catch (JsonException) { return null; }
        catch (KeyNotFoundException) { return null; }
    }

    private static Result Failure(string code, ErrorType type = ErrorType.Validation) => Result.Failure(new AppError(code, type));
}
