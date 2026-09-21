// SPDX-License-Identifier: MIT
using System.DirectoryServices.Protocols;
using System.Net;
using Auditarium.Bll.Abstractions.Identity;

namespace Auditarium.Infrastructure.Ldap;

public sealed class LdapUserDirectory : IUserDirectory
{
    public Task ValidateConnectionAsync(LdapProviderConnectionSettings settings, CancellationToken cancellationToken = default) =>
        Task.Run(() =>
        {
            using var connection = CreateConnection(settings);
            connection.Bind();
            ValidateSearchAndMappings(connection, settings);
        }, cancellationToken);

    public Task<DirectoryUser?> AuthenticateAsync(LdapProviderConnectionSettings settings, string login, string password, CancellationToken cancellationToken = default) =>
        Task.Run(() =>
        {
            using var serviceConnection = CreateConnection(settings);
            serviceConnection.Bind();
            var found = FindUser(serviceConnection, settings, login, true);
            if (found is null) return null;
            using var userConnection = CreateConnection(settings, new NetworkCredential(found.DistinguishedName, password));
            userConnection.Bind();
            return new DirectoryUser(found.ExternalId, login.Trim(), found.DisplayName, found.Email);
        }, cancellationToken);

    private static LdapConnection CreateConnection(LdapProviderConnectionSettings settings, NetworkCredential? credential = null)
    {
        var identifier = new LdapDirectoryIdentifier(settings.Host, settings.Port, false, false);
        var connection = new LdapConnection(identifier, credential ?? new NetworkCredential(settings.BindUser ?? string.Empty, settings.BindPassword ?? string.Empty), AuthType.Basic)
        {
            Timeout = settings.ConnectionTimeout,
            SessionOptions = { ProtocolVersion = 3 }
        };
        if (settings.TlsMode == LdapTlsMode.Ldaps) connection.SessionOptions.SecureSocketLayer = true;
        if (settings.TlsMode == LdapTlsMode.StartTls) connection.SessionOptions.StartTransportLayerSecurity(null);
        return connection;
    }

    private static FoundUser? FindUser(LdapConnection connection, LdapProviderConnectionSettings settings, string login, bool requireOne)
    {
        var filter = $"(&({settings.LoginAttribute}={EscapeFilter(login)}){settings.SearchFilter})";
        var request = new SearchRequest(settings.BaseDn, filter, SearchScope.Subtree, [settings.StableExternalIdAttribute, settings.DisplayNameAttribute, settings.EmailAttribute]);
        var response = (SearchResponse)connection.SendRequest(request);
        if (response.Entries.Count == 0) return null;
        if (requireOne && response.Entries.Count != 1) throw new InvalidOperationException("LDAP search returned more than one matching identity.");
        var entry = response.Entries[0];
        return new FoundUser(entry.DistinguishedName,
            Attribute(entry, settings.StableExternalIdAttribute) ?? throw new InvalidOperationException("LDAP stable external ID attribute is missing."),
            Attribute(entry, settings.DisplayNameAttribute) ?? login,
            Attribute(entry, settings.EmailAttribute));
    }

    private static void ValidateSearchAndMappings(LdapConnection connection, LdapProviderConnectionSettings settings)
    {
        var request = new SearchRequest(settings.BaseDn, settings.SearchFilter, SearchScope.Subtree,
            [settings.StableExternalIdAttribute, settings.DisplayNameAttribute, settings.EmailAttribute])
        {
            SizeLimit = 1
        };
        var response = (SearchResponse)connection.SendRequest(request);
        if (response.Entries.Count == 0) return;
        var entry = response.Entries[0];
        _ = Attribute(entry, settings.StableExternalIdAttribute) ?? throw new InvalidOperationException("LDAP stable external ID attribute is missing.");
        _ = Attribute(entry, settings.DisplayNameAttribute) ?? throw new InvalidOperationException("LDAP display name attribute is missing.");
    }

    private static string? Attribute(SearchResultEntry entry, string name) => entry.Attributes[name]?.GetValues(typeof(string)).Cast<string>().SingleOrDefault();
    private static string EscapeFilter(string value) => value.Replace("\\", "\\5c", StringComparison.Ordinal).Replace("*", "\\2a", StringComparison.Ordinal).Replace("(", "\\28", StringComparison.Ordinal).Replace(")", "\\29", StringComparison.Ordinal).Replace("\0", "\\00", StringComparison.Ordinal);
    private sealed record FoundUser(string DistinguishedName, string ExternalId, string DisplayName, string? Email);
}
