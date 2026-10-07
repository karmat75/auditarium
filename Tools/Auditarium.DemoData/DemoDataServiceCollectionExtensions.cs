// SPDX-License-Identifier: MIT
using Auditarium.Bll;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Dal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auditarium.DemoData;

public static class DemoDataServiceCollectionExtensions
{
    public static IServiceCollection AddAuditariumDemoData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();
        services.AddAuditariumBll();
        services.AddAuditariumPersistence(configuration);
        services.AddScoped<DemoDataCurrentActor>();
        services.AddScoped<ICurrentActor>(provider => provider.GetRequiredService<DemoDataCurrentActor>());
        services.AddScoped<DemoFixtureRunner>();
        return services;
    }
}
