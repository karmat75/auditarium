// SPDX-License-Identifier: MIT
using Xunit;
using Auditarium.Infrastructure.Ldap;

namespace Auditarium.Persistence.IntegrationTests.Ldap;

[Collection(OpenLdapCollection.Name)]
public sealed class OpenLdapFixtureTests(OpenLdapFixture ldap)
{
    [Fact]
    public async Task Directory_adapter_connection_diagnostic_has_no_directory_side_effects()
    {
        var directory = new LdapUserDirectory();
        await directory.ValidateConnectionAsync(ldap.Settings);
        var user = await directory.AuthenticateAsync(ldap.Settings, "auditor", OpenLdapFixture.TestUserPassword);

        Assert.NotNull(user);
        Assert.Equal("auditor", user.ExternalId);
    }

    [Fact]
    public async Task Directory_adapter_authenticates_and_maps_a_stable_external_identity()
    {
        var user = await new LdapUserDirectory().AuthenticateAsync(ldap.Settings, "auditor", OpenLdapFixture.TestUserPassword);

        Assert.NotNull(user);
        Assert.Equal("auditor", user.ExternalId);
        Assert.Equal("LDAP Auditor", user.DisplayName);
        Assert.Equal("auditor@auditarium.test", user.Email);
    }
}
