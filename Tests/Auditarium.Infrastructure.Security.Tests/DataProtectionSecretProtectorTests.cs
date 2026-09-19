// SPDX-License-Identifier: MIT
using Auditarium.Infrastructure.Security;
using Microsoft.AspNetCore.DataProtection;
using Xunit;

namespace Auditarium.Infrastructure.Security.Tests;

public sealed class DataProtectionSecretProtectorTests
{
    [Fact]
    public void Protect_and_unprotect_round_trip_uses_versioned_envelope()
    {
        var protector = CreateProtector();
        var encrypted = protector.Protect("secret-value", "Mail:Smtp:Password");

        Assert.StartsWith("audsec:v1:", encrypted, StringComparison.Ordinal);
        Assert.Equal("secret-value", protector.Unprotect(encrypted, "Mail:Smtp:Password"));
    }

    [Fact]
    public void Unprotect_rejects_a_different_purpose()
    {
        var protector = CreateProtector();
        var encrypted = protector.Protect("secret-value", "Mail:Smtp:Password");

        Assert.ThrowsAny<Exception>(() => protector.Unprotect(encrypted, "Webhook:Secret"));
    }

    [Fact]
    public void Unprotect_rejects_an_unknown_envelope_version()
    {
        var protector = CreateProtector();

        Assert.ThrowsAny<Exception>(() => protector.Unprotect("audsec:v2:value", "Mail:Smtp:Password"));
    }

    private static DataProtectionSecretProtector CreateProtector()
    {
        var directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "auditarium-tests", Guid.NewGuid().ToString("N")));
        return new DataProtectionSecretProtector(DataProtectionProvider.Create(directory, builder => builder.SetApplicationName("Auditarium.Tests")));
    }
}
