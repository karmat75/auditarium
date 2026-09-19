// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Security;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
namespace Auditarium.Infrastructure.Security;

public sealed class DataProtectionSecretProtector(IDataProtectionProvider provider) : ISecretProtector
{
    private const string Prefix = "audsec:v1:";
    public string Protect(string plaintext, string purpose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext); ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        return Prefix + provider.CreateProtector("Auditarium", "SettingsSecrets", purpose).Protect(plaintext);
    }
    public string Unprotect(string protectedValue, string purpose)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedValue); ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        if (!protectedValue.StartsWith(Prefix, StringComparison.Ordinal)) throw new CryptographicException("Unsupported Auditarium secret envelope.");
        return provider.CreateProtector("Auditarium", "SettingsSecrets", purpose).Unprotect(protectedValue[Prefix.Length..]);
    }
}
