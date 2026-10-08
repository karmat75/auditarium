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
using Auditarium.Web.Components;
using Auditarium.Web.Pages.Analysis;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Mediator;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Auditarium.Infrastructure.Security.Tests;

public sealed class PresentationFoundationTests
{
    [Theory]
    [InlineData(Auditarium.Models.Catalog.AuditState.Draft, "Entwurf", "neutral")]
    [InlineData(Auditarium.Models.Catalog.AuditState.InProgress, "In Bearbeitung", "warning")]
    [InlineData(Auditarium.Models.Catalog.AuditState.Finalized, "Abgeschlossen", "success")]
    [InlineData(Auditarium.Models.Catalog.DocumentUsageState.Deprecated, "Veraltet", "warning")]
    public void Status_presentation_uses_consistent_semantic_labels(object status, string label, string tone)
    {
        var presentation = StatusPresentations.Resolve(status);

        Assert.Equal(label, presentation.Label);
        Assert.Equal(tone, presentation.Tone);
    }

    [Fact]
    public void Analysis_filter_uses_the_shared_bll_filter_contract()
    {
        var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var input = new AnalysisFilterInputModel { CreatedFrom = from, ScopeTypeId = 2, AuditUnitId = 3, DocumentId = 4, CatalogVersionId = 5, AuditState = Auditarium.Models.Catalog.AuditState.Finalized };

        var filter = input.ToBllFilter();

        Assert.Equal(from, filter.CreatedFrom);
        Assert.Equal(2, filter.ScopeTypeId);
        Assert.Equal(3, filter.AuditUnitId);
        Assert.Equal(4, filter.DocumentId);
        Assert.Equal(5, filter.CatalogVersionId);
        Assert.Equal(Auditarium.Models.Catalog.AuditState.Finalized, filter.AuditState);
    }

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

    [Fact]
    public async Task Runtime_default_deny_rejects_a_request_without_security_declaration()
    {
        var events = new RecordingAuditEvents();
        var behavior = new AuthorizationBehavior<UndeclaredProbeRequest, Result>(
            new StubCurrentActor(), new StubPermissionEvaluator(new HashSet<string>(StringComparer.Ordinal)), events,
            NullLogger<AuthorizationBehavior<UndeclaredProbeRequest, Result>>.Instance);
        var handlerCalled = false;

        var result = await behavior.Handle(new UndeclaredProbeRequest(), (_, _) =>
        {
            handlerCalled = true;
            return ValueTask.FromResult(Result.Success());
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AUTHORIZATION.SECURITY_DECLARATION_REQUIRED", Assert.Single(result.Errors).Code);
        Assert.False(handlerCalled);
        Assert.Equal("ACCESS_DENIED", Assert.Single(events.Events).Action);
    }

    [Fact]
    public async Task Forced_password_change_blocks_normal_requests_even_when_permission_exists()
    {
        var events = new RecordingAuditEvents();
        var behavior = new AuthorizationBehavior<ProtectedProbeRequest, Result>(
            new StubCurrentActor(mustChangePassword: true),
            new StubPermissionEvaluator(new HashSet<string>(["Users.Manage"], StringComparer.Ordinal)),
            events,
            NullLogger<AuthorizationBehavior<ProtectedProbeRequest, Result>>.Instance);
        var handlerCalled = false;

        var result = await behavior.Handle(new ProtectedProbeRequest(), (_, _) =>
        {
            handlerCalled = true;
            return ValueTask.FromResult(Result.Success());
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AUTHENTICATION.PASSWORD_CHANGE_REQUIRED", Assert.Single(result.Errors).Code);
        Assert.False(handlerCalled);
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
    public async Task Successful_login_is_not_returned_when_required_event_logging_fails()
    {
        var handler = new AuthenticateInteractiveUserCommandHandler(
            new StubRouter("LOCAL"),
            new StubLocalAuthentication(new AuthenticationSuccess(42, false)),
            new StubLdapAuthentication(),
            new ThrowingAuditEvents());

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await handler.Handle(new AuthenticateInteractiveUserCommand("alice", "secret", null), CancellationToken.None));
    }

    [Fact]
    public async Task Audit_logging_failure_cannot_turn_denied_access_into_success()
    {
        var behavior = new AuthorizationBehavior<ProtectedProbeRequest, Result>(
            new StubCurrentActor(),
            new StubPermissionEvaluator(new HashSet<string>(StringComparer.Ordinal)),
            new ThrowingAuditEvents(),
            NullLogger<AuthorizationBehavior<ProtectedProbeRequest, Result>>.Instance);
        var handlerCalled = false;

        var result = await behavior.Handle(new ProtectedProbeRequest(), (_, _) =>
        {
            handlerCalled = true;
            return ValueTask.FromResult(Result.Success());
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AUTHORIZATION.FORBIDDEN", Assert.Single(result.Errors).Code);
        Assert.False(handlerCalled);
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

    [Fact]
    public void Tabulator_grid_foundation_keeps_shared_and_page_specific_concerns_separate()
    {
        var root = FindRepositoryRoot();
        var tables = File.ReadAllText(Path.Combine(root, "UI", "Auditarium.Web", "wwwroot", "js", "tables.js"));
        var auditUnits = File.ReadAllText(Path.Combine(root, "UI", "Auditarium.Web", "Pages", "AuditUnits", "Index.cshtml"));
        var auditUnitDetails = File.ReadAllText(Path.Combine(root, "UI", "Auditarium.Web", "Pages", "AuditUnits", "Details.cshtml"));
        var auditUnitEdit = File.ReadAllText(Path.Combine(root, "UI", "Auditarium.Web", "Pages", "AuditUnits", "Edit.cshtml"));
        var layout = File.ReadAllText(Path.Combine(root, "UI", "Auditarium.Web", "Pages", "Shared", "_Layout.cshtml"));
        var siteCss = File.ReadAllText(Path.Combine(root, "UI", "Auditarium.Web", "wwwroot", "css", "site.css"));

        Assert.DoesNotContain("Audit Unit löschen", tables, StringComparison.Ordinal);
        Assert.DoesNotContain("delete-audit-unit-modal", tables, StringComparison.Ordinal);
        Assert.Contains("params.sort?.[0]", tables, StringComparison.Ordinal);
        Assert.Contains("ResizeObserver", tables, StringComparison.Ordinal);
        Assert.Contains("layout: isTree ? \"fitColumns\" : \"fitDataStretch\"", tables, StringComparison.Ordinal);
        Assert.Contains("queueRedrawForChangedWidth", tables, StringComparison.Ordinal);
        Assert.Contains("paginationElement: footer || false", tables, StringComparison.Ordinal);
        Assert.Contains("arrangeExternalPagination", tables, StringComparison.Ordinal);
        Assert.Contains("Array.from(footer.children)", tables, StringComparison.Ordinal);
        Assert.DoesNotContain("footer.querySelectorAll(\".tabulator-page, .tabulator-pages\")", tables, StringComparison.Ordinal);
        Assert.DoesNotContain("responsiveLayout: \"collapse\"", tables, StringComparison.Ordinal);
        Assert.Contains("card-tools", auditUnits, StringComparison.Ordinal);
        Assert.Contains("aud-grid-search", auditUnits, StringComparison.Ordinal);
        Assert.Contains("Suche zurücksetzen", auditUnits, StringComparison.Ordinal);
        Assert.Contains("aud-grid-actions", auditUnits, StringComparison.Ordinal);
        Assert.Contains("audit-units.js", auditUnits, StringComparison.Ordinal);
        Assert.Contains("RenderSectionAsync(\"Scripts\"", layout, StringComparison.Ordinal);
        Assert.Contains("aud-grid-footer", siteCss, StringComparison.Ordinal);
        Assert.Contains("aud-grid-pagination", siteCss, StringComparison.Ordinal);
        Assert.Contains("not(.aud-grid-search)", siteCss, StringComparison.Ordinal);
        Assert.Contains("data-aud-tree=\"true\"", auditUnits, StringComparison.Ordinal);
        Assert.DoesNotContain("data-aud-tabulator-footer", auditUnits, StringComparison.Ordinal);
        Assert.Contains("pagination: !isTree", tables, StringComparison.Ordinal);
        Assert.Contains("table.on(\"dataLoading\"", tables, StringComparison.Ordinal);
        Assert.Contains("table.on(\"dataProcessed\"", tables, StringComparison.Ordinal);
        Assert.Contains("row.isTreeExpanded()", tables, StringComparison.Ordinal);
        Assert.Contains("row.treeCollapse()", tables, StringComparison.Ordinal);
        Assert.Contains("window.scrollTo(0", tables, StringComparison.Ordinal);
        Assert.Contains("actionsFormatter", tables, StringComparison.Ordinal);
        Assert.Contains("makeTreeControlsAccessible", tables, StringComparison.Ordinal);
        Assert.Contains("syncTreeControlState", tables, StringComparison.Ordinal);
        Assert.DoesNotContain("\"linkField\":\"detailsUrl\"", auditUnits, StringComparison.Ordinal);
        Assert.Contains("\"widthGrow\":1", auditUnits, StringComparison.Ordinal);
        Assert.Contains("\"editUrl\"", auditUnits, StringComparison.Ordinal);
        Assert.Contains("asp-page=\"Edit\"", auditUnitDetails, StringComparison.Ordinal);
        Assert.Contains("@if (Model.CanManage)", auditUnitDetails, StringComparison.Ordinal);
        Assert.DoesNotContain("_AuditUnitForm", auditUnitDetails, StringComparison.Ordinal);
        Assert.Contains("_AuditUnitForm", auditUnitEdit, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Auditarium.sln"))) return directory.FullName;
        throw new DirectoryNotFoundException("Could not locate the Auditarium repository root.");
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

    private sealed record UndeclaredProbeRequest : IRequest<Result>;

    [Auditarium.Bll.Security.RequiresPermission("Users.Manage")]
    private sealed record ProtectedProbeRequest : IRequest<Result>;

    private sealed class StubCurrentActor(bool mustChangePassword = false) : ICurrentActor
    {
        public ActorType Type => ActorType.User;
        public long? UserId => 42;
        public bool IsAuthenticated => true;
        public bool MustChangePassword => mustChangePassword;
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

    private sealed class ThrowingAuditEvents : IAuditEventWriter
    {
        public Task WriteAsync(AuditEvent eventData, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Injected audit-log write failure.");
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
