// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Administration.Roles;
using Auditarium.Bll.Features.Administration.Settings;
using Auditarium.Bll.Features.Administration.Users;
using Auditarium.Bll.Features.Identity.ApiCredentials;
using Auditarium.Bll.Features.Identity.AuthenticationProviders;
using Auditarium.Bll.Features.Jobs;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Auditarium.Api;

public static class AdministrationEndpoints
{
    public static IEndpointRouteBuilder MapAdministrationEndpoints(this IEndpointRouteBuilder routes)
    {
        var users = routes.MapGroup("/api/v1/users").RequireAuthorization().WithTags("Administration - Users");
        users.MapGet("/", ListUsers).Produces<PageResponse<UserListItem>>().Produces<ProblemDetails>(400);
        users.MapPost("/", CreateUser).Produces<long>(201).Produces<ProblemDetails>(400);
        users.MapGet("/{userId:long}", GetUser).Produces<UserDetails>().Produces<ProblemDetails>(404);
        users.MapPut("/{userId:long}", UpdateUser).Produces(204).Produces<ProblemDetails>(409);
        users.MapPost("/{userId:long}/roles/{roleId:long}", AddUserRole).Produces(204);
        users.MapDelete("/{userId:long}/roles/{roleId:long}", RemoveUserRole).Produces(204);
        users.MapPost("/{userId:long}/identities", AddIdentity).Produces<long>(201);
        users.MapPost("/{userId:long}/local-identity", ProvisionLocalIdentity).Produces<LocalCredentialIssued>(201).Produces<ProblemDetails>(409);
        users.MapGet("/{userId:long}/api-credentials", ListCredentials).Produces<IReadOnlyList<ApiCredentialMetadata>>();
        routes.MapPost("/api/v1/local-identities/{identityId:long}/credential/reset", ResetLocalCredential).RequireAuthorization().WithTags("Administration - Local Credentials").Produces<LocalCredentialIssued>().Produces<ProblemDetails>(409);
        routes.MapPost("/api/v1/api-identities/{identityId:long}/credentials", CreateCredential).RequireAuthorization().WithTags("Administration - API Credentials").Produces<ApiCredentialCreated>(201);

        var credentials = routes.MapGroup("/api/v1/api-credentials").RequireAuthorization().WithTags("Administration - API Credentials");
        credentials.MapPost("/{credentialId:long}/rotate", RotateCredential).Produces<ApiCredentialCreated>(201);
        credentials.MapPost("/{credentialId:long}/revoke", RevokeCredential).Produces(204);

        var roles = routes.MapGroup("/api/v1/roles").RequireAuthorization().WithTags("Administration - Roles");
        roles.MapGet("/", ListRoles).Produces<IReadOnlyList<RoleListItem>>();
        roles.MapPost("/", CreateRole).Produces<long>(201);
        roles.MapGet("/{roleId:long}", GetRole).Produces<RoleDetails>();
        roles.MapPut("/{roleId:long}", UpdateRole).Produces(204).Produces<ProblemDetails>(409);
        roles.MapPost("/{roleId:long}/permissions/{permissionKey}", AddPermission).Produces(204);
        roles.MapDelete("/{roleId:long}/permissions/{permissionKey}", RemovePermission).Produces(204);

        var providers = routes.MapGroup("/api/v1/authentication-providers").RequireAuthorization().WithTags("Administration - Authentication Providers");
        providers.MapGet("/", ListProviders).Produces<IReadOnlyList<AuthenticationProviderListItem>>();
        providers.MapPost("/ldap", CreateLdapProvider).Produces<long>(201);
        providers.MapGet("/{providerId:long}", GetProvider).Produces<AuthenticationProviderDetails>();
        providers.MapPut("/{providerId:long}", UpdateLdapProvider).Produces(204).Produces<ProblemDetails>(409);
        providers.MapPost("/{providerId:long}/enabled", SetProviderEnabled).Produces(204).Produces<ProblemDetails>(409);
        providers.MapPost("/{providerId:long}/connection-test", TestProvider).Produces(204).Produces<ProblemDetails>(400);
        providers.MapDelete("/{providerId:long}/settings/{property}", ResetProviderSetting).Produces(204).Produces<ProblemDetails>(409);
        providers.MapDelete("/{providerId:long}", DeleteProvider).Produces(204).Produces<ProblemDetails>(409);

        var settings = routes.MapGroup("/api/v1/settings").RequireAuthorization().WithTags("Administration - Settings");
        settings.MapGet("/", ListSettings).Produces<IReadOnlyList<SettingDetails>>();
        settings.MapPut("/{key}", UpdateSetting).Produces(204).Produces<ProblemDetails>(409);

        var jobs = routes.MapGroup("/api/v1/jobs").RequireAuthorization().WithTags("Administration - Jobs");
        jobs.MapGet("/", ListJobs).WithName("ListJobs").WithSummary("Lists operational state for known jobs.").Produces<IReadOnlyList<JobOperationsItem>>().Produces<ProblemDetails>(403);
        jobs.MapPost("/{jobKey}/trigger", TriggerJob).WithName("TriggerJob").WithSummary("Requests a permitted manual job trigger.").Produces(202).Produces<ProblemDetails>(400).Produces<ProblemDetails>(403).Produces<ProblemDetails>(404).Produces<ProblemDetails>(409);
        return routes;
    }

    private static async Task<IResult> ListUsers(IMediator mediator, string? search, bool? isActive, int page = 1, int pageSize = 50, string? sort = null, CancellationToken ct = default)
    {
        var paging = new PageRequest(page, pageSize).Validate(); if (!paging.IsSuccess) return ApiProblemDetails.From(paging);
        var parsedSort = SortRequest.Parse(sort, new HashSet<string>(StringComparer.Ordinal) { "username", "displayName", "email", "isActive" }); if (!parsedSort.IsSuccess) return ApiProblemDetails.From(parsedSort);
        var order = parsedSort.Value ?? new SortRequest("username", SortDirection.Ascending);
        var result = await mediator.Send(new ListUsersQuery(search, isActive, (page - 1) * pageSize, pageSize, order.Field, order.Direction == SortDirection.Descending), ct);
        return result.IsSuccess ? Results.Ok(new PageResponse<UserListItem>(result.Value!.Items, page, pageSize, result.Value.TotalCount)) : ApiProblemDetails.From(result);
    }
    private static async Task<IResult> CreateUser(IMediator mediator, UserContract contract, CancellationToken ct) { var result = await mediator.Send(new CreateUserCommand(contract.Username, contract.DisplayName, contract.Email, contract.IsActive), ct); return result.IsSuccess ? Results.Created($"/api/v1/users/{result.Value}", result.Value) : ApiProblemDetails.From(result); }
    private static async Task<IResult> GetUser(IMediator mediator, long userId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new GetUserQuery(userId), ct));
    private static async Task<IResult> UpdateUser(IMediator mediator, long userId, UserContract contract, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new UpdateUserCommand(userId, contract.Username, contract.DisplayName, contract.Email, contract.IsActive, contract.ConcurrencyVersion), ct));
    private static async Task<IResult> AddUserRole(IMediator mediator, long userId, long roleId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new AddUserRoleCommand(userId, roleId), ct));
    private static async Task<IResult> RemoveUserRole(IMediator mediator, long userId, long roleId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new RemoveUserRoleCommand(userId, roleId), ct));
    private static async Task<IResult> AddIdentity(IMediator mediator, long userId, IdentityContract contract, CancellationToken ct) { var result = await mediator.Send(new AddUserIdentityCommand(userId, contract.ProviderId, contract.ExternalId), ct); return result.IsSuccess ? Results.Created($"/api/v1/users/{userId}", result.Value) : ApiProblemDetails.From(result); }
    private static async Task<IResult> ProvisionLocalIdentity(IMediator mediator, long userId, CancellationToken ct)
    {
        var result = await mediator.Send(new ProvisionLocalIdentityCommand(userId), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/users/{userId}", result.Value) : ApiProblemDetails.From(result);
    }
    private static async Task<IResult> ResetLocalCredential(IMediator mediator, long identityId, LocalCredentialResetContract contract, CancellationToken ct) =>
        ApiProblemDetails.From(await mediator.Send(new ResetLocalCredentialCommand(identityId, contract.ConcurrencyVersion), ct));
    private static async Task<IResult> ListCredentials(IMediator mediator, long userId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new ListApiCredentialsQuery(userId), ct));
    private static async Task<IResult> CreateCredential(IMediator mediator, long identityId, ApiCredentialContract contract, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateApiCredentialCommand(identityId, contract.Name, contract.ExpiresAt), ct);
        return result.IsSuccess ? Results.Created($"/api/v1/api-credentials/{result.Value!.CredentialId}", result.Value) : ApiProblemDetails.From(result);
    }
    private static async Task<IResult> RotateCredential(IMediator mediator, long credentialId, RotateApiCredentialContract contract, CancellationToken ct) { var result = await mediator.Send(new RotateApiCredentialCommand(credentialId, contract.Name, contract.ExpiresAt), ct); return result.IsSuccess ? Results.Created($"/api/v1/api-credentials/{result.Value!.CredentialId}", result.Value) : ApiProblemDetails.From(result); }
    private static async Task<IResult> RevokeCredential(IMediator mediator, long credentialId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new RevokeApiCredentialCommand(credentialId), ct));

    private static async Task<IResult> ListRoles(IMediator mediator, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new ListRolesQuery(), ct));
    private static async Task<IResult> CreateRole(IMediator mediator, RoleContract contract, CancellationToken ct) { var result = await mediator.Send(new CreateRoleCommand(contract.Name, contract.Description, contract.IsActive), ct); return result.IsSuccess ? Results.Created($"/api/v1/roles/{result.Value}", result.Value) : ApiProblemDetails.From(result); }
    private static async Task<IResult> GetRole(IMediator mediator, long roleId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new GetRoleQuery(roleId), ct));
    private static async Task<IResult> UpdateRole(IMediator mediator, long roleId, RoleContract contract, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new UpdateRoleCommand(roleId, contract.Name, contract.Description, contract.IsActive, contract.ConcurrencyVersion), ct));
    private static async Task<IResult> AddPermission(IMediator mediator, long roleId, string permissionKey, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new AddPermissionToRoleCommand(roleId, permissionKey), ct));
    private static async Task<IResult> RemovePermission(IMediator mediator, long roleId, string permissionKey, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new RemovePermissionFromRoleCommand(roleId, permissionKey), ct));

    private static async Task<IResult> ListProviders(IMediator mediator, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new ListAuthenticationProvidersQuery(), ct));
    private static async Task<IResult> CreateLdapProvider(IMediator mediator, CreateLdapProviderContract contract, CancellationToken ct) { var result = await mediator.Send(new CreateLdapProviderCommand(contract.ProviderKey, contract.DisplayName), ct); return result.IsSuccess ? Results.Created($"/api/v1/authentication-providers/{result.Value}", result.Value) : ApiProblemDetails.From(result); }
    private static async Task<IResult> GetProvider(IMediator mediator, long providerId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new GetAuthenticationProviderQuery(providerId), ct));
    private static async Task<IResult> UpdateLdapProvider(IMediator mediator, long providerId, UpdateLdapProviderContract contract, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new UpdateLdapProviderCommand(providerId, contract.DisplayName, contract.Values, contract.SettingVersions, contract.ConcurrencyVersion), ct));
    private static async Task<IResult> SetProviderEnabled(IMediator mediator, long providerId, EnabledContract contract, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new SetAuthenticationProviderEnabledCommand(providerId, contract.IsEnabled, contract.ConcurrencyVersion), ct));
    private static async Task<IResult> TestProvider(IMediator mediator, long providerId, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new TestLdapProviderConnectionCommand(providerId), ct));
    private static async Task<IResult> ResetProviderSetting(IMediator mediator, long providerId, string property, long concurrencyVersion, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new ResetLdapProviderSettingCommand(providerId, property, concurrencyVersion), ct));
    private static async Task<IResult> DeleteProvider(IMediator mediator, long providerId, long concurrencyVersion, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new DeleteAuthenticationProviderCommand(providerId, concurrencyVersion), ct));

    private static async Task<IResult> ListSettings(IMediator mediator, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new ListSettingsQuery(), ct));
    private static async Task<IResult> UpdateSetting(IMediator mediator, string key, SettingContract contract, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new UpdateSettingCommand(key, contract.Value, contract.ConcurrencyVersion), ct));
    private static async Task<IResult> ListJobs(IMediator mediator, CancellationToken ct) => ApiProblemDetails.From(await mediator.Send(new ListJobOperationsQuery(), ct));
    private static async Task<IResult> TriggerJob(IMediator mediator, string jobKey, CancellationToken ct)
    {
        var result = await mediator.Send(new TriggerJobCommand(jobKey), ct);
        return result.IsSuccess ? Results.Accepted($"/api/v1/jobs/{Uri.EscapeDataString(jobKey)}") : ApiProblemDetails.From(result);
    }
}

public sealed record UserContract(string Username, string DisplayName, string? Email, bool IsActive, long ConcurrencyVersion = 0);
public sealed record IdentityContract(long ProviderId, string ExternalId);
public sealed record LocalCredentialResetContract(long ConcurrencyVersion);
public sealed record ApiCredentialContract(string Name, DateTimeOffset? ExpiresAt);
public sealed record RotateApiCredentialContract(string Name, DateTimeOffset? ExpiresAt);
public sealed record RoleContract(string Name, string? Description, bool IsActive, long ConcurrencyVersion = 0);
public sealed record CreateLdapProviderContract(string ProviderKey, string DisplayName);
public sealed record UpdateLdapProviderContract(string DisplayName, Dictionary<string, string?> Values, Dictionary<string, long> SettingVersions, long ConcurrencyVersion);
public sealed record EnabledContract(bool IsEnabled, long ConcurrencyVersion);
public sealed record SettingContract(string Value, long ConcurrencyVersion);
