// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Auditarium.Infrastructure.Security;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditariumDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var keyRingPath = configuration["Auditarium:DataProtection:KeyRingPath"];
        var applicationName = configuration["Auditarium:DataProtection:ApplicationName"];
        if (string.IsNullOrWhiteSpace(keyRingPath) || string.IsNullOrWhiteSpace(applicationName)) throw new InvalidOperationException("Auditarium:DataProtection:KeyRingPath and ApplicationName are required.");
        services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keyRingPath)).SetApplicationName(applicationName);
        services.AddSingleton<ISecretProtector, DataProtectionSecretProtector>();
        return services;
    }
}
