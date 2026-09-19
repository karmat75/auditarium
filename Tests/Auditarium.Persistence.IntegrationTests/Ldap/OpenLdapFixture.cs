// SPDX-License-Identifier: MIT
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Auditarium.Bll.Abstractions.Identity;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests.Ldap;

/// <summary>
/// A disposable LDAP directory with deterministic test identities. Future LDAP adapter
/// tests consume its host configuration instead of relying on a developer directory.
/// </summary>
public sealed class OpenLdapFixture : IAsyncLifetime
{
    public const string Domain = "auditarium.test";
    public const string BaseDn = "dc=auditarium,dc=test";
    public const string AdminDn = "cn=admin," + BaseDn;
    public const string AdminPassword = "auditarium-test-admin";
    public const string TestUserDn = "uid=auditor,ou=people," + BaseDn;
    public const string TestUserPassword = "auditarium-test-user";

    private readonly IContainer _container = new ContainerBuilder("osixia/openldap:1.5.0")
        .WithEnvironment("LDAP_ORGANISATION", "Auditarium test directory")
        .WithEnvironment("LDAP_DOMAIN", Domain)
        .WithEnvironment("LDAP_ADMIN_PASSWORD", AdminPassword)
        .WithEnvironment("LDAP_CONFIG_PASSWORD", "auditarium-test-config")
        .WithPortBinding(389, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(389))
        .Build();

    public string Host => _container.Hostname;
    public ushort Port => _container.GetMappedPublicPort(389);
    public string LdapUrl => $"ldap://{Host}:{Port}";
    public LdapProviderConnectionSettings Settings => new(Host, Port, LdapTlsMode.None, TimeSpan.FromSeconds(15), AdminDn, AdminPassword, BaseDn, "uid", "(objectClass=inetOrgPerson)", "uid", "cn", "mail");
    public Task<ExecResult> ExecuteSearchAsync(string command) =>
        _container.ExecAsync(["/bin/sh", "-c", command]);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var consecutiveReadyChecks = 0;
        for (var attempt = 0; attempt < 40; attempt++)
        {
            var ready = await _container.ExecAsync(["/bin/sh", "-c", $"ldapwhoami -x -H ldap://127.0.0.1:389 -D '{AdminDn}' -w '{AdminPassword}'"]);
            consecutiveReadyChecks = ready.ExitCode == 0 ? consecutiveReadyChecks + 1 : 0;
            if (consecutiveReadyChecks == 3) break;
            if (attempt == 39) throw new InvalidOperationException("The LDAP test fixture did not become ready in time.");
            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }
        var ldif = $"dn: ou=people,{BaseDn}\nobjectClass: organizationalUnit\nou: people\n\n" +
                   $"dn: {TestUserDn}\nobjectClass: inetOrgPerson\ncn: LDAP Auditor\nsn: Auditor\nuid: auditor\nmail: auditor@{Domain}\nuserPassword: {TestUserPassword}\n";
        var escaped = ldif.Replace("'", "'\\\"'\\\"'");
        var result = await _container.ExecAsync(["/bin/sh", "-c", $"printf '%s' '{escaped}' | ldapadd -x -H ldap://127.0.0.1:389 -D '{AdminDn}' -w '{AdminPassword}'"]);
        if (result.ExitCode != 0) throw new InvalidOperationException($"The LDAP test fixture could not seed its deterministic directory entries (exit {result.ExitCode}): {result.Stderr}");
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public sealed class OpenLdapCollection : ICollectionFixture<OpenLdapFixture>
{
    public const string Name = "OpenLDAP";
}
