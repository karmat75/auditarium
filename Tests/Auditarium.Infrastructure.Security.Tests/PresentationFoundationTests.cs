// SPDX-License-Identifier: MIT
using Auditarium.Api;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Features.Administration.Users;
using Auditarium.Bll.Features.Identity.LocalCredentials;
using Auditarium.Bll.Features.Jobs;
using Auditarium.Bll.Jobs;
using Auditarium.Bll.Pipeline;
using Auditarium.Common.Results;
using Auditarium.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Mediator;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Auditarium.Infrastructure.Security.Tests;

public sealed class PresentationFoundationTests
{
    [Fact]
    public void Every_mediator_request_has_exactly_one_security_declaration()
    {
        var requestTypes = typeof(Auditarium.Bll.Features.System.GetHostStatus.GetHostStatusQuery).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false } && type.GetInterfaces().Any(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IRequest<>)));

        foreach (var requestType in requestTypes)
        {
            var declarations = requestType.GetCustomAttributes(false).Count(attribute => attribute is Auditarium.Bll.Security.RequiresPermissionAttribute or Auditarium.Bll.Security.AllowAnonymousAttribute or Auditarium.Bll.Security.AllowPasswordChangeAttribute);
            Assert.True(declarations == 1, $"{requestType.FullName} has {declarations} security declarations.");
        }
    }

    [Fact]
    public void Local_credential_administration_requires_user_and_authentication_permissions()
    {
        var declaration = Assert.Single(typeof(Auditarium.Bll.Features.Administration.Users.ProvisionLocalIdentityCommand)
            .GetCustomAttributes(false).OfType<Auditarium.Bll.Security.RequiresPermissionAttribute>());

        Assert.Equal(["Users.Manage", "Authentication.Manage"], declaration.Permissions);
    }

    [Fact]
    public async Task Local_credential_administration_is_denied_when_only_one_required_permission_is_present()
    {
        var events = new RecordingAuditEvents();
        var behavior = new AuthorizationBehavior<ProvisionLocalIdentityCommand, Result<LocalCredentialIssued>>(
            new StubCurrentActor(), new StubPermissionEvaluator(new HashSet<string>(["Users.Manage"], StringComparer.Ordinal)), events,
            NullLogger<AuthorizationBehavior<ProvisionLocalIdentityCommand, Result<LocalCredentialIssued>>>.Instance);
        var handlerCalled = false;

        var result = await behavior.Handle(new ProvisionLocalIdentityCommand(42), (_, _) =>
        {
            handlerCalled = true;
            return ValueTask.FromResult(Result<LocalCredentialIssued>.Success(new(1, "alice", "temporary", 1)));
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AUTHORIZATION.FORBIDDEN", Assert.Single(result.Errors).Code);
        Assert.False(handlerCalled);
        Assert.Equal("ACCESS_DENIED", Assert.Single(events.Events).Action);
    }

    [Theory]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.Unauthorized, 401)]
    [InlineData(ErrorType.Forbidden, 403)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.Failure, 500)]
    public void AppError_type_maps_to_the_required_http_status(ErrorType type, int expectedStatus)
    {
        Assert.Equal(expectedStatus, ApiProblemDetails.StatusFor(type));
    }

    [Fact]
    public void Field_target_is_preserved_in_web_model_state()
    {
        var result = Result.Failure(new AppError("AUTHENTICATION.PASSWORD_REQUIRED", ErrorType.Validation, "Input.NewPassword"));
        var modelState = new ModelStateDictionary();

        result.ApplyTo(modelState);

        Assert.True(modelState.ContainsKey("Input.NewPassword"));
        Assert.Contains("Aktuelles und neues Passwort", modelState["Input.NewPassword"]!.Errors[0].ErrorMessage, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, 50, "API.PAGINATION.PAGE_INVALID")]
    [InlineData(1, 201, "API.PAGINATION.PAGE_SIZE_INVALID")]
    public void Invalid_pagination_is_a_validation_error(int page, int pageSize, string code)
    {
        var result = new PageRequest(page, pageSize).Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal(code, result.Errors[0].Code);
        Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
    }

    [Fact]
    public async Task Interactive_login_uses_the_bll_authentication_path_and_records_success()
    {
        var events = new RecordingAuditEvents();
        var handler = new AuthenticateInteractiveUserCommandHandler(
            new StubRouter("LOCAL"),
            new StubLocalAuthentication(new AuthenticationSuccess(42, true)),
            new StubLdapAuthentication(),
            events);

        var result = await handler.Handle(new AuthenticateInteractiveUserCommand("alice", "secret", null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value!.UserId);
        Assert.True(result.Value.MustChangePassword);
        Assert.Equal(new AuditEvent("LOGIN", "User", 42, 42), Assert.Single(events.Events));
    }

    [Fact]
    public async Task Failed_interactive_login_is_an_expected_error_and_is_recorded()
    {
        var events = new RecordingAuditEvents();
        var handler = new AuthenticateInteractiveUserCommandHandler(
            new StubRouter(null),
            new StubLocalAuthentication(null),
            new StubLdapAuthentication(),
            events);

        var result = await handler.Handle(new AuthenticateInteractiveUserCommand("alice", "secret", null), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(new AppError("AUTHENTICATION.FAILED", ErrorType.Unauthorized), Assert.Single(result.Errors));
        Assert.Equal(new AuditEvent("LOGIN_FAILED", "Authentication"), Assert.Single(events.Events));
    }

    [Fact]
    public void Sort_contract_accepts_only_endpoint_allowlisted_fields()
    {
        var allowedFields = new HashSet<string>(StringComparer.Ordinal) { "name" };

        var accepted = SortRequest.Parse("-name", allowedFields);
        var rejected = SortRequest.Parse("createdAt", allowedFields);

        Assert.True(accepted.IsSuccess);
        Assert.Equal(new SortRequest("name", SortDirection.Descending), accepted.Value);
        Assert.False(rejected.IsSuccess);
        Assert.Equal("API.SORT.FIELD_NOT_ALLOWED", rejected.Errors[0].Code);
    }

    [Fact]
    public async Task Manual_job_trigger_uses_the_coordinator_and_records_the_requesting_actor()
    {
        var coordinator = new RecordingJobCoordinator(Result.Success());
        var events = new RecordingAuditEvents();
        var handler = new JobOperationsHandler(new StubJobRegistry(), null!, null!, null!, coordinator, events);

        var result = await handler.Handle(new TriggerJobCommand(JobKeys.Retention), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal((JobKeys.Retention, JobTrigger.Manual), Assert.Single(coordinator.Requests));
        var auditEvent = Assert.Single(events.Events);
        Assert.Equal("JOB_TRIGGERED", auditEvent.Action);
        Assert.Equal("MaintenanceJob", auditEvent.ObjectType);
        Assert.Equal("Retention", auditEvent.AfterState!["job_key"]);
        Assert.Equal("MANUAL", auditEvent.AfterState["trigger"]);
    }

    [Fact]
    public async Task Rejected_manual_job_trigger_does_not_write_a_trigger_audit_event()
    {
        var coordinator = new RecordingJobCoordinator(Result.Failure(new AppError("JOB.ALREADY_RUNNING", ErrorType.Conflict)));
        var events = new RecordingAuditEvents();
        var handler = new JobOperationsHandler(new StubJobRegistry(), null!, null!, null!, coordinator, events);

        var result = await handler.Handle(new TriggerJobCommand(JobKeys.Retention), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("JOB.ALREADY_RUNNING", Assert.Single(result.Errors).Code);
        Assert.Empty(events.Events);
    }

    private sealed class StubRouter(string? provider) : IAuthenticationRouter
    {
        public Task<string?> RouteAsync(string login, string? explicitlySelectedProvider, CancellationToken cancellationToken = default) =>
            Task.FromResult(provider);
    }

    private sealed class StubLocalAuthentication(AuthenticationSuccess? success) : ILocalAuthenticationService
    {
        public Task<AuthenticationSuccess?> AuthenticateAsync(AuthenticationAttempt attempt, CancellationToken cancellationToken = default) =>
            Task.FromResult(success);

        public Task<LocalPasswordChangeStatus> ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default) =>
            Task.FromResult(LocalPasswordChangeStatus.InvalidCredential);

        public Task<TemporaryLocalCredential> CreateTemporaryCredentialAsync(long identityId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new TemporaryLocalCredential("temporary", 1));

        public Task<LocalCredentialResetResult> ResetTemporaryCredentialAsync(long identityId, long concurrencyVersion, CancellationToken cancellationToken = default) =>
            Task.FromResult(new LocalCredentialResetResult(LocalCredentialResetStatus.NotFound));
    }

    private sealed class StubLdapAuthentication : ILdapAuthenticationService
    {
        public Task<AuthenticationSuccess?> AuthenticateAsync(string providerKey, AuthenticationAttempt attempt, CancellationToken cancellationToken = default) =>
            Task.FromResult<AuthenticationSuccess?>(null);

        public Task ValidateConnectionAsync(string providerKey, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubCurrentActor : ICurrentActor
    {
        public ActorType Type => ActorType.User;
        public long? UserId => 42;
        public bool IsAuthenticated => true;
    }

    private sealed class StubPermissionEvaluator(IReadOnlySet<string> permissions) : IPermissionEvaluator
    {
        public Task<bool> HasPermissionAsync(long userId, string permission, CancellationToken cancellationToken = default) =>
            Task.FromResult(permissions.Contains(permission));

        public Task<IReadOnlySet<string>> GetPermissionsAsync(long userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(permissions);
    }

    private sealed class RecordingAuditEvents : IAuditEventWriter
    {
        public List<AuditEvent> Events { get; } = [];

        public Task WriteAsync(AuditEvent eventData, CancellationToken cancellationToken = default)
        {
            Events.Add(eventData);
            return Task.CompletedTask;
        }
    }

    private sealed class StubJobRegistry : IJobRegistry
    {
        public IReadOnlyList<JobDefinition> Definitions { get; } = [new(JobKeys.Retention, JobTrigger.Scheduled | JobTrigger.Manual)];
        public bool TryGet(string jobKey, out JobDefinition? definition)
        {
            definition = Definitions.SingleOrDefault(x => x.JobKey == jobKey);
            return definition is not null;
        }
    }

    private sealed class RecordingJobCoordinator(Result result) : IJobCoordinator
    {
        public List<(string JobKey, JobTrigger Trigger)> Requests { get; } = [];
        public Task<Result> RunAsync(string jobKey, JobTrigger trigger, CancellationToken cancellationToken = default)
        {
            Requests.Add((jobKey, trigger));
            return Task.FromResult(result);
        }
    }
}
