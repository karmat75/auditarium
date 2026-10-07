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

    public sealed record FixtureRequirement(
        string Topic,
        string Reference,
        string Requirement,
        string Question,
        AuditQuestionResult InitialResult,
        string? Comment,
        string? Evidence);
}
