# ADR 0004 – Authentication Provider Instance Model

## Status

Accepted

## Context

Users can authenticate through distinct sources, including more than one LDAP configuration, without fragmenting the user and authorization model.

## Decision

Authentication is modeled through concrete provider instances. A user identity references a provider instance and stable `external_id`. LOCAL and API are system-managed singleton instances; multiple LDAP provider instances are supported. Provider-specific settings use the established effective configuration model, and routing operates on concrete provider instances. The model permits later provider types without a parallel user or authorization model.

## Consequences

Identity uniqueness and provider routing are explicit, while roles remain attached to the common user model. New provider types can be added through this extension point, but are not implied to be implemented by this ADR.

## References

- [identity entities](../../Core/Auditarium.Models/Identity/IdentityEntities.cs), [authentication provider use cases](../../Core/Auditarium.Bll/Features/Identity/AuthenticationProviders/AuthenticationProviderCommands.cs), and [effective setting resolver](../../Persistence/Auditarium.Dal/Settings/ApplicationSettingResolver.cs)
- [authentication services and router](../../Persistence/Auditarium.Dal/Identity/AuthenticationServices.cs) and [LDAP implementation](../../Infrastructure/Auditarium.Infrastructure.Ldap/)
- [administration workflow tests](../../Tests/Auditarium.Persistence.IntegrationTests/AdministrationWorkflowIntegrationTests.cs) and [LDAP fixture tests](../../Tests/Auditarium.Persistence.IntegrationTests/Ldap/OpenLdapFixtureTests.cs)
