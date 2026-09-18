// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Pipeline;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Auditarium.Bll;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditariumBll(this IServiceCollection services)
    {
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        services.AddValidatorsFromAssemblyContaining<Features.System.GetHostStatus.GetHostStatusHandler>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ObservabilityBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }

    public static IServiceCollection AddAuditariumAnonymousActor(this IServiceCollection services)
    {
        services.AddScoped<ICurrentActor, AnonymousCurrentActor>();
        services.AddScoped<IPermissionEvaluator, DenyAllPermissionEvaluator>();
        return services;
    }

    private sealed class AnonymousCurrentActor : ICurrentActor
    {
        public ActorType Type => ActorType.Anonymous;
        public long? UserId => null;
        public bool IsAuthenticated => false;
    }

    private sealed class DenyAllPermissionEvaluator : IPermissionEvaluator
    {
        public Task<bool> HasPermissionAsync(long userId, string permission, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<IReadOnlySet<string>> GetPermissionsAsync(long userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlySet<string>>(new HashSet<string>());
    }
}
