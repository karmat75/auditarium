// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Files;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auditarium.Fal;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditariumFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var rootPath = configuration["Auditarium:Storage:RootPath"];
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new InvalidOperationException("Auditarium:Storage:RootPath is required.");
        }

        services.AddSingleton<IFileStorage>(new LocalFileStorage(rootPath));
        return services;
    }
}
