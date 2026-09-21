// SPDX-License-Identifier: MIT
using System.ComponentModel.DataAnnotations;

namespace Auditarium.Web.Pages.Administration.Settings;

public sealed class SettingInputModel
{
    [Required] public string Key { get; set; } = string.Empty;
    [Required] public string Value { get; set; } = string.Empty;
    public long ConcurrencyVersion { get; set; }
}
