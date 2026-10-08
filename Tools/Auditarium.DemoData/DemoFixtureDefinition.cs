// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Models.Catalog;

namespace Auditarium.DemoData;

/// <summary>The single, versioned source of the reproducible development fixture.</summary>
public static class DemoFixtureDefinition
{
    public const int FixtureVersion = 1;
    public const string MarkerSource = "auditarium-demo://fixture/v1";
    public const string BuildingMarker = "Auditarium demo fixture v1: BUILDING";
    public const string CompleteMarker = "Auditarium demo fixture v1: COMPLETE";
    public const string BuildingTitle = "Auditarium Demo Fixture v1 (BUILDING)";
    public const string CompleteTitle = "Auditarium Demo Fixture v1 – Serverraum-Referenzfall";
    public const string FixtureNote = "auditarium-demo://fixture/v1";

    public const string OrganizationName = "Musterorganisation";
    public const string SiteName = "Standort Musterstadt";
    public const string BuildingName = "Gebäude A";
    public const string ServerRoomName = "Serverraum R-01";
    public const string RetiredRoomName = "Serverraum R-00 (außer Betrieb)";

    public const string InitialAuditName = "Serverraum R-01 – Erstprüfung";
    public const string RepeatAuditName = "Serverraum R-01 – Wiederholung";
    public const string DraftAuditName = "Serverraum R-01 – Vorbereitung";
    public const string InProgressAuditName = "Serverraum R-01 – laufende Prüfung";
    public const string CanceledAuditName = "Serverraum R-01 – abgebrochene Prüfung";
    public const int ExpectedAuditUnitCount = 60;

    public static readonly AuditSettings ResponsePolicies = new(new Dictionary<AuditQuestionResult, ResponseRule>
    {
        [AuditQuestionResult.Yes] = new(false, false),
        [AuditQuestionResult.No] = new(true, false),
        [AuditQuestionResult.NotApplicable] = new(true, false),
        [AuditQuestionResult.NotDeterminable] = new(true, true)
    });

    public static readonly IReadOnlyList<FixtureRequirement> Requirements =
    [
        new("Zuständigkeit und Dokumentation", "R01", "Für den Raum müssen eine verantwortliche Person und eine Vertretung benannt sein.", "Ist eine verantwortliche Person für den Raum benannt?", AuditQuestionResult.Yes, null, null),
        new("Zutritt und Schlüssel", "R03", "Zutrittsberechtigungen müssen durch die verantwortliche Person freigegeben sein.", "Sind alle aufgeführten Zutrittsberechtigungen freigegeben?", AuditQuestionResult.No, "Für eine Berechtigung liegt keine Freigabe vor.", null),
        new("Fremdzutritt und Arbeiten", "R06", "Wartungsarbeiten müssen vor Beginn freigegeben sein.", "Waren die Freigabeunterlagen zum Prüfzeitpunkt zugänglich?", AuditQuestionResult.NotDeterminable, "Die Freigabeunterlagen sind nicht zugänglich.", "Prüfnotiz: Unterlagen angefordert."),
        new("Kontrollen und Ausstattung", "R10", "Wassereintrittssensorik muss geprüft werden, falls sie installiert ist.", "Liegt eine Funktionsprüfung der installierten Sensorik vor?", AuditQuestionResult.NotApplicable, "Im Raum ist keine Wassereintrittssensorik installiert.", null)
    ];

    public static readonly IReadOnlyList<FixtureAuditUnit> AuditUnits =
    [
        new(OrganizationName, "ORGANIZATION", null, AuditUnitUsageState.Active),
        new(SiteName, "SITE", OrganizationName, AuditUnitUsageState.Active),
        new(BuildingName, "BUILDING", SiteName, AuditUnitUsageState.Active),
        new(ServerRoomName, "TECHNICAL_AREA", BuildingName, AuditUnitUsageState.Active),
        new(RetiredRoomName, "TECHNICAL_AREA", BuildingName, AuditUnitUsageState.Inactive, "Außer Betrieb; nur als historischer Demo-Kontext."),
        new("Gebäude B", "BUILDING", SiteName, AuditUnitUsageState.Active),
        new("Gebäude C (außer Betrieb)", "BUILDING", SiteName, AuditUnitUsageState.Inactive, "Wird derzeit nicht genutzt."),
        new("Bereich IT-Betrieb", "AREA", BuildingName, AuditUnitUsageState.Active),
        new("Technikbereich Energieversorgung", "TECHNICAL_AREA", BuildingName, AuditUnitUsageState.Active),
        new("Raum A-101 Service Desk", "ROOM", "Bereich IT-Betrieb", AuditUnitUsageState.Active),
        new("Raum A-102 Netzwerkwerkstatt", "ROOM", "Bereich IT-Betrieb", AuditUnitUsageState.Active),
        new("Raum A-103 Ersatzteillager", "ROOM", "Bereich IT-Betrieb", AuditUnitUsageState.Active),
        new("Raum A-104 Archiv (gesperrt)", "ROOM", "Bereich IT-Betrieb", AuditUnitUsageState.Inactive, "Zutritt bis zur Sanierung gesperrt."),
        new("Netzwerk Gebäude A", "NETWORK", "Technikbereich Energieversorgung", AuditUnitUsageState.Active),
        new("USV-Monitoring", "IT_SYSTEM", "Netzwerk Gebäude A", AuditUnitUsageState.Active),
        new("Anwendung Energie-Monitoring", "APPLICATION", "USV-Monitoring", AuditUnitUsageState.Active),
        new("Bereich Logistik", "AREA", "Gebäude B", AuditUnitUsageState.Active),
        new("Bereich Produktion", "AREA", "Gebäude B", AuditUnitUsageState.Active),
        new("Raum B-001 Wareneingang", "ROOM", "Bereich Logistik", AuditUnitUsageState.Active),
        new("Raum B-002 Kommissionierung", "ROOM", "Bereich Logistik", AuditUnitUsageState.Active),
        new("Raum B-003 Verpackung", "ROOM", "Bereich Logistik", AuditUnitUsageState.Active),
        new("Raum B-004 Versand", "ROOM", "Bereich Logistik", AuditUnitUsageState.Active),
        new("Raum B-101 Fertigung Nord", "ROOM", "Bereich Produktion", AuditUnitUsageState.Active),
        new("Raum B-102 Fertigung Süd", "ROOM", "Bereich Produktion", AuditUnitUsageState.Active),
        new("Raum B-103 Qualitätsprüfung", "ROOM", "Bereich Produktion", AuditUnitUsageState.Active),
        new("Raum B-104 Materialbereitstellung", "ROOM", "Bereich Produktion", AuditUnitUsageState.Active),
        new("Prozess Wareneingang", "PROCESS", "Bereich Logistik", AuditUnitUsageState.Active),
        new("Standort Nordhafen", "SITE", OrganizationName, AuditUnitUsageState.Active),
        new("Gebäude Nord 1", "BUILDING", "Standort Nordhafen", AuditUnitUsageState.Active),
        new("Gebäude Nord 2", "BUILDING", "Standort Nordhafen", AuditUnitUsageState.Active),
        new("Bereich Verwaltung Nord", "AREA", "Gebäude Nord 1", AuditUnitUsageState.Active),
        new("Technikbereich Colocation", "TECHNICAL_AREA", "Gebäude Nord 1", AuditUnitUsageState.Active),
        new("Raum N-101 Empfang", "ROOM", "Bereich Verwaltung Nord", AuditUnitUsageState.Active),
        new("Raum N-102 Personal", "ROOM", "Bereich Verwaltung Nord", AuditUnitUsageState.Active),
        new("Raum N-103 Besprechung Elbe", "ROOM", "Bereich Verwaltung Nord", AuditUnitUsageState.Active),
        new("Raum N-104 Besprechung Hafen", "ROOM", "Bereich Verwaltung Nord", AuditUnitUsageState.Active),
        new("Netzwerk Nordhafen", "NETWORK", "Technikbereich Colocation", AuditUnitUsageState.Active),
        new("Gateway Nordhafen", "IT_SYSTEM", "Netzwerk Nordhafen", AuditUnitUsageState.Active),
        new("Anwendung Besucheranmeldung", "APPLICATION", "Gateway Nordhafen", AuditUnitUsageState.Active),
        new("Bereich Warenumschlag", "AREA", "Gebäude Nord 2", AuditUnitUsageState.Active),
        new("Raum N-201 Anlieferung", "ROOM", "Bereich Warenumschlag", AuditUnitUsageState.Active),
        new("Raum N-202 Zollabwicklung", "ROOM", "Bereich Warenumschlag", AuditUnitUsageState.Active),
        new("Raum N-203 Kühlzone", "ROOM", "Bereich Warenumschlag", AuditUnitUsageState.Active),
        new("Raum N-204 Gefahrgut", "ROOM", "Bereich Warenumschlag", AuditUnitUsageState.Active),
        new("Raum N-205 Leergut", "ROOM", "Bereich Warenumschlag", AuditUnitUsageState.Active),
        new("Prozess Versand Nord", "PROCESS", "Bereich Warenumschlag", AuditUnitUsageState.Active),
        new("Standort Südpark", "SITE", OrganizationName, AuditUnitUsageState.Active),
        new("Gebäude Süd 1", "BUILDING", "Standort Südpark", AuditUnitUsageState.Active),
        new("Gebäude Süd 2", "BUILDING", "Standort Südpark", AuditUnitUsageState.Active),
        new("Bereich Forschung", "AREA", "Gebäude Süd 1", AuditUnitUsageState.Active),
        new("Bereich Schulung", "AREA", "Gebäude Süd 1", AuditUnitUsageState.Active),
        new("Raum S-101 Labor Biometrie", "ROOM", "Bereich Forschung", AuditUnitUsageState.Active),
        new("Raum S-102 Labor Elektronik", "ROOM", "Bereich Forschung", AuditUnitUsageState.Active),
        new("Raum S-103 Testfeld", "ROOM", "Bereich Forschung", AuditUnitUsageState.Active),
        new("Raum S-201 Schulung Alpha", "ROOM", "Bereich Schulung", AuditUnitUsageState.Active),
        new("Raum S-202 Schulung Beta", "ROOM", "Bereich Schulung", AuditUnitUsageState.Active),
        new("Raum S-203 Schulung Gamma", "ROOM", "Bereich Schulung", AuditUnitUsageState.Active),
        new("Technikbereich Kälteversorgung", "TECHNICAL_AREA", "Gebäude Süd 2", AuditUnitUsageState.Active),
        new("Netzwerk Südpark", "NETWORK", "Technikbereich Kälteversorgung", AuditUnitUsageState.Active),
        new("Sensorik Südpark", "IT_SYSTEM", "Netzwerk Südpark", AuditUnitUsageState.Active)
    ];

    public sealed record FixtureRequirement(
        string Topic,
        string Reference,
        string Requirement,
        string Question,
        AuditQuestionResult InitialResult,
        string? Comment,
        string? Evidence);

    public sealed record FixtureAuditUnit(
        string Name,
        string ScopeTypeKey,
        string? ParentName,
        AuditUnitUsageState UsageState,
        string? UsageStateReason = null);
}
