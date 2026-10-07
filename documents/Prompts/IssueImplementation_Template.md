Implementiere ausschließlich GitHub Issue #<ISSUE> – <TITLE>.

Der Issue ist die verbindliche Quelle für den ausgewählten Arbeitsumfang und
seine Akzeptanzkriterien.

Lies zuerst `AGENTS.md` und beachte alle dort festgelegten Architektur-,
Coding-, Test- und Model-/Reasoning-Regeln. Lies anschließend den ausgewählten
Issue vollständig, den übergeordneten oder Abhängigkeitskontext soweit relevant,
`documents/README.md` sowie die einschlägigen Architektur-Seiten und ADRs.
Prüfe den aktuellen Code, relevante Tests und `git status`; erhalte vorhandene
oder fremde Änderungen.

Implementiere nur den Scope des ausgewählten Issue. Beginne nach dessen
Abschluss nicht automatisch mit dem nächsten Issue. Leite keine neue Arbeit aus
dem eingefrorenen Soll-/Pflichtenheft ab; nutze historische Inhalte nur als
Provenienz oder Beleg, wenn sie für den Issue relevant sind. Vermeide
opportunistische Refactorings außerhalb des Issue.

Fehlt eine notwendige Produkt- oder Architekturentscheidung, stoppe und
berichte sie. Widerspricht die aktuelle Implementierung materiell einem
akzeptierten ADR, stoppe und berichte den Widerspruch, statt die Entscheidung
stillschweigend zu ändern. Aktualisiere Tests und Dokumentation, wenn die
ausgewählte Änderung dies erfordert. Erstelle oder aktualisiere ein ADR nur,
wenn die Governance-Schwelle tatsächlich erreicht ist. Sicherheits- und
Default-Deny-Grenzen bleiben erhalten; schwäche Akzeptanzkriterien nicht ab,
nur damit Tests bestehen.

Führe nach der Implementierung die relevanten Prüfungen aus, mindestens soweit
anwendbar:

```bash
dotnet restore Auditarium.sln --locked-mode
dotnet build Auditarium.sln --configuration Release --no-restore
dotnet test Auditarium.sln --configuration Release --no-build --no-restore
dotnet format Auditarium.sln --verify-no-changes --no-restore
```

Fehlt der Zugriff auf die Docker Engine, prüfe zunächst außerhalb der Sandbox
`docker version` und `docker context show`.

Unter Windows mit Docker Desktop und WSL müssen Testcontainers-
Integrationstests außerhalb der Sandbox ausgeführt werden. Wenn `docker
version` dort einen Server und `docker context show` den aktiven Docker-Desktop-
Kontext (typischerweise `desktop-linux`) ausgibt, verwende diesen Kontext
unverändert und entferne eine eventuell gesetzte `DOCKER_HOST`-Variable nur für
die aktuelle PowerShell-Sitzung:

```powershell
Remove-Item Env:DOCKER_HOST -ErrorAction SilentlyContinue
docker version
docker context show
dotnet test Auditarium.sln --configuration Release --no-build --no-restore
```

Setze `DOCKER_HOST` nicht automatisch aus `docker context inspect`: Die vom
Docker-CLI angezeigte Named-Pipe-Notation kann von Testcontainers nicht als
gültiger NPipe-URI verarbeitet werden. Erst wenn Docker außerhalb der Sandbox
selbst keinen Server erreicht, darf der Docker-Desktop-/WSL-Zugriff weiter
diagnostiziert werden.

Keine dauerhafte Änderung von Benutzer-, System- oder Repository-Konfiguration
ohne ausdrücklichen Auftrag.

Beende den Lauf mit einem kurzen Bericht:

```text
Status
→ completed | partially completed | blocked

Issue
→ #<ISSUE> – <TITLE>

Implemented
→ principal results

Necessary changes outside the obvious implementation files
→ none | list and justify

Tests / verification
→ commands and results

Acceptance criteria
→ each criterion: satisfied | not satisfied

Documentation / ADR impact
→ none | exact files/decisions updated

Blockers / deviations
→ none | exact details

Scope
→ confirm that only the selected Issue was implemented and no next Issue was started
```

Wenn die Akzeptanzkriterien des ausgewählten Issue erfüllt sind: **STOP**.
