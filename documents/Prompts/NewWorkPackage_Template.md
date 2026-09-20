Wir beginnen mit Work Package <Nummer> aus dem Projekt Auditarium.

Verbindliche Quelle ist:
documents/Auditarium_Soll_Pflichtenheft.md

Lies zuerst AGENTS.md sowie ausschließlich:
- Kapitel 20.<Nummer> zum Work Package,
- die dort fachlich referenzierten Kapitel,
- Kapitel 19 zu Tests und Abnahmekriterien.

Setze das Work Package vollständig im Repository um.
Halte dich strikt an die Architektur- und Sicherheitsvorgaben des Pflichtenhefts.
Triff keine neue Produktentscheidung stillschweigend. Wenn eine verbindliche Entscheidung fehlt, halte an dieser Stelle an und frage gezielt nach.

Ändere das Pflichtenheft nur, wenn wir eine neue oder korrigierte Anforderung entscheiden. Dann Versionsnummer erhöhen und Änderungshistorie ergänzen.

Prüfe die zugehörigen Abnahmekriterien mit angemessenen Tests.
Für containerbasierte Builds, Tests und insbesondere Testcontainers-Integrationstests darfst du den Docker-Socket des Entwicklungssystems verwenden. Binde ihn bei Bedarf gezielt in den SDK-Container ein (`-v /var/run/docker.sock:/var/run/docker.sock`), damit Testcontainer auf dem Entwicklungssystem gestartet werden können. Diese Freigabe gilt ausschließlich für Entwicklungs- und Testcontainer im Rahmen dieses Work Packages; entferne von dir erzeugte temporäre Container nach Abschluss.
Führe vor dem Abschluss zwingend den projektweiten CI-nahen Format-Check mit `sh scripts/verify-format.sh` aus und behebe alle Befunde. Prüfe bei neu erzeugten oder von Werkzeugen generierten Textdateien, insbesondere EF-Core-Migrationen, zusätzlich das Dateiformat: Die Dateien müssen UTF-8 ohne BOM verwenden. Führe anschließend einen vollständigen Release-Build sowie die passenden Tests aus.
Berichte abschließend:
- geänderte Dateien,
- durchgeführte Prüfungen,
- erfüllte Abnahmekriterien,
- verbleibende offene Entscheidungen.
