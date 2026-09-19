// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Auditarium.Infrastructure.Ldap;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditariumLdap(this IServiceCollection services) => services.AddScoped<IUserDirectory, LdapUserDirectory>();
}
