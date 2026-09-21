// SPDX-License-Identifier: MIT
using Auditarium.Api;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Features.Identity.LocalCredentials;
using Auditarium.Common.Results;
using Auditarium.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Mediator;
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

    private sealed class StubRouter(string? provider) : IAuthenticationRouter
    {
        public Task<string?> RouteAsync(string login, string? explicitlySelectedProvider, CancellationToken cancellationToken = default) =>
            Task.FromResult(provider);
    }

    private sealed class StubLocalAuthentication(AuthenticationSuccess? success) : ILocalAuthenticationService
    {
        public Task<AuthenticationSuccess?> AuthenticateAsync(AuthenticationAttempt attempt, CancellationToken cancellationToken = default) =>
            Task.FromResult(success);

        public Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }

    private sealed class StubLdapAuthentication : ILdapAuthenticationService
    {
        public Task<AuthenticationSuccess?> AuthenticateAsync(string providerKey, AuthenticationAttempt attempt, CancellationToken cancellationToken = default) =>
            Task.FromResult<AuthenticationSuccess?>(null);

        public Task ValidateConnectionAsync(string providerKey, CancellationToken cancellationToken = default) => Task.CompletedTask;
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
}
