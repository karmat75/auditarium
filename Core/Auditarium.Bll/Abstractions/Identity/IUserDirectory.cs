// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Abstractions.Identity;

public enum LdapTlsMode { None, StartTls, Ldaps }

public sealed record LdapProviderConnectionSettings(
    string Host, int Port, LdapTlsMode TlsMode, TimeSpan ConnectionTimeout,
    string? BindUser, string? BindPassword, string BaseDn, string LoginAttribute,
    string SearchFilter, string StableExternalIdAttribute, string DisplayNameAttribute,
    string EmailAttribute);

public sealed record DirectoryUser(string ExternalId, string Username, string DisplayName, string? Email);
public interface IUserDirectory
{
    Task<DirectoryUser?> AuthenticateAsync(LdapProviderConnectionSettings settings, string login, string password, CancellationToken cancellationToken = default);
    Task ValidateConnectionAsync(LdapProviderConnectionSettings settings, CancellationToken cancellationToken = default);
}
