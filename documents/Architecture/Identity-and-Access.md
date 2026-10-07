# Identität und Zugriff

Auditarium hat ein gemeinsames Benutzermodell. Identitäten binden einen Benutzer an eine konkrete Authentifizierungsprovider-Instanz und deren stabilen `external_id`. `LOCAL` und `API` sind systemverwaltete Providerinstanzen; mehrere LDAP-Instanzen können nebeneinander bestehen. Provider-Einstellungen werden über das etablierte Modell wirksamer Konfiguration aufgelöst, und die Authentifizierungsroute wählt konkrete Providerinstanzen. LOCAL-Credentials und API-Credentials gehören zu den jeweiligen Identitäten.

Weitere Providertypen kann dieses Providerinstanz-Modell aufnehmen; OIDC oder SAML sind jedoch keine aktuell dokumentierte Funktionalität. Siehe [ADR 0004](../ADR/0004-authentication-provider-instance-model.md).

Autorisierung ist permission-basiert: Im Code definierte Berechtigungen werden über Rollen aggregiert und Rollen Benutzern zugewiesen. BLL-Use-Cases erklären ihren Bedarf, `AuthorizationBehavior` erzwingt ihn zentral. Fehlende oder nicht erfüllte Deklarationen führen zu Default-Deny; ein inaktiver Benutzer besitzt bei späteren autorisierten Use-Cases keine wirksamen Berechtigungen.

Der System Actor arbeitet mit `user_id = 0` und ausschließlich den explizit zugewiesenen `SYSTEM_INTERNAL`-Berechtigungen. Web und API führen kein konkurrierendes Berechtigungsmodell. Dieses Modell ist in [ADR 0005](../ADR/0005-permission-based-rbac-and-default-deny-authorization.md) festgelegt. Die Wiederherstellung des Default-Administrators ist ein Betreiberverfahren im [Bootstrap- und Recovery-Runbook](../Operations/BootstrapAndRecovery.md).

Die ausschließlich explizit auslösbare Development-Provisionierung der regulären Rollen für `DEFAULT_ADMIN` ist als technische Tool-Ausnahme begrenzt; sie ändert weder die normale Administration noch die Bootstrap-Regel. Siehe [ADR 0011](../ADR/0011-development-demo-data-invocation-and-admin-provisioning.md).
