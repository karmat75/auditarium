Bearbeite ausschließlich **Work Package <WP>** aus `documents/Auditarium_Soll_Pflichtenheft.md`.

Lies zuerst `AGENTS.md` und beachte alle dort festgelegten Architektur-, Coding-, Test- und Model-/Reasoning-Regeln.

Lies anschließend das ausgewählte Work Package vollständig sowie gezielt alle fachlichen und technischen Kapitel des Soll-/Pflichtenhefts, die für seine Umsetzung erforderlich sind. Das Soll-/Pflichtenheft ist die normative Quelle. Die Änderungshistorie ist kein zweiter Sollzustand. Offene Punkte aus Kapitel 21 dürfen nicht eigenmächtig entschieden werden.

Prüfe vor Änderungen den bestehenden Code, relevante Tests und `git status`. Erhalte vorhandene oder fremde Änderungen.

Implementiere nur den Scope des ausgewählten Work Packages. Ziehe keine Funktionen späterer Work Packages vor und beginne nach Abschluss nicht automatisch mit dem nächsten Work Package.

Für `20.9.x` gilt zusätzlich:
- Beachte die Einleitung von Kapitel `20.9`.
- Web verwendet Razor Pages → Mediator → BLL.
- API verwendet HTTP → Mediator → BLL.
- Web verwendet niemals die eigene API als Backend.
- Web und API verwenden dieselben fachlichen BLL-Use-Cases.
- Keine Businesslogik in Razor Pages oder API-Endpunkten duplizieren.
- Berechtigungen bleiben serverseitig in BLL/Authorization erzwungen.
- Web-Schreibformulare verwenden InputModels, PRG und Antiforgery.
- EF-/Domain-Entities sind keine Web-ViewModels oder öffentlichen API-Verträge.
- API-Fehler verwenden das zentrale `AppError` → `ProblemDetails`-Mapping.
- Concurrency-Konflikte dürfen keine Fremdänderungen überschreiben.
- Filter und Sortierung nur über explizit freigegebene Felder.
- OpenAPI beschreibt die tatsächlich implementierte API.
- Die im Soll-/Pflichtenheft definierte Grenze zum nächsten `20.9.x`-Teilpaket ist verbindlich.

Falls für die Umsetzung ein kleiner technischer oder BLL-seitiger Baustein fehlt, ergänze ihn nur dann, wenn er zwingend erforderlich ist und das gewünschte Verhalten im Soll-/Pflichtenheft bereits eindeutig festgelegt ist. Keine neuen Produktentscheidungen und keine opportunistischen Refactorings.

Wenn das aktuell gewählte Modell oder Reasoning-Level gemäß `AGENTS.md` für eine große oder riskante Änderung nicht ausreicht, stoppe vor der Implementierung und nenne knapp das empfohlene Modell und Reasoning-Level.

Führe nach der Implementierung die relevanten Prüfungen aus, mindestens soweit anwendbar:

```bash
dotnet restore Auditarium.sln --locked-mode
dotnet build Auditarium.sln --configuration Release --no-restore
dotnet test Auditarium.sln --configuration Release --no-build --no-restore
dotnet format Auditarium.sln --verify-no-changes --no-restore
```

Fehlt dir an irgendeiner Stelle der Zugriff auf die Docker Engine, verwende den Docker Socket des Host Systems.

Schwäche keine Abnahmekriterien ab, nur damit Tests grün werden.

Beende den Lauf mit einem kurzen Bericht:

```text
Status
→ abgeschlossen | teilweise abgeschlossen | blockiert

Umgesetzt
→ wichtigste Ergebnisse

Zusätzliche notwendige Änderungen außerhalb des direkten WP-Scope
→ keine | exakt benennen und begründen

Tests / Verifikation
→ ausgeführte Kommandos und Ergebnis

Abnahmekriterien
→ je Kriterium: erfüllt | nicht erfüllt

Blocker / Abweichungen
→ keine | konkret benennen

Scope
→ bestätigen, dass kein nachfolgendes Work Package vorgezogen wurde
```

Wenn die Abnahmekriterien des Work Package erfüllt sind: **STOP**.
