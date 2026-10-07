// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Bll.Features.Catalog;
using Auditarium.Common.Results;
using Auditarium.Dal;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.DemoData;

public sealed class DemoDataFixtureException(string message) : InvalidOperationException(message);

public enum DemoFixtureApplyStatus { Applied, AlreadyPresent }

public sealed record DemoFixtureApplyResult(DemoFixtureApplyStatus Status, int FixtureVersion);

public sealed class DemoFixtureRunner(AuditariumDbContext db, IMediator mediator)
{
    public async Task ValidatePreflightAsync(CancellationToken cancellationToken = default)
    {
        var preflight = await InspectAsync(cancellationToken);
        ThrowIfPreflightRejected(preflight);
    }

    public async Task<DemoFixtureApplyResult> ApplyAsync(
        CancellationToken cancellationToken = default,
        Func<CancellationToken, Task>? afterBuildingMarker = null)
    {
        var preflight = await InspectAsync(cancellationToken);
        if (preflight == FixturePreflight.Complete)
            return new(DemoFixtureApplyStatus.AlreadyPresent, DemoFixtureDefinition.FixtureVersion);
        ThrowIfPreflightRejected(preflight);

        var documentId = await SendIdAsync(new CreateDocumentCommand(new(
            DemoFixtureDefinition.BuildingTitle,
            "Auditarium",
            $"Demo fixture v{DemoFixtureDefinition.FixtureVersion}",
            null,
            DemoFixtureDefinition.MarkerSource,
            DocumentUsageState.Active,
            null,
            DemoFixtureDefinition.BuildingMarker)), cancellationToken);

        if (afterBuildingMarker is not null) await afterBuildingMarker(cancellationToken);

        var technicalArea = await ScopeTypeIdAsync("TECHNICAL_AREA", cancellationToken);
        var organization = await SendIdAsync(new CreateAuditUnitCommand(new(null, 1, DemoFixtureDefinition.OrganizationName, null, AuditUnitUsageState.Active, null, DemoFixtureDefinition.FixtureNote)), cancellationToken);
        var site = await SendIdAsync(new CreateAuditUnitCommand(new(organization, 2, DemoFixtureDefinition.SiteName, null, AuditUnitUsageState.Active, null, DemoFixtureDefinition.FixtureNote)), cancellationToken);
        var building = await SendIdAsync(new CreateAuditUnitCommand(new(site, 3, DemoFixtureDefinition.BuildingName, null, AuditUnitUsageState.Active, null, DemoFixtureDefinition.FixtureNote)), cancellationToken);
        var serverRoom = await SendIdAsync(new CreateAuditUnitCommand(new(building, technicalArea, DemoFixtureDefinition.ServerRoomName, null, AuditUnitUsageState.Active, null, DemoFixtureDefinition.FixtureNote)), cancellationToken);
        await SendIdAsync(new CreateAuditUnitCommand(new(building, technicalArea, DemoFixtureDefinition.RetiredRoomName, null, AuditUnitUsageState.Inactive, "Außer Betrieb; nur als historischer Demo-Kontext.", DemoFixtureDefinition.FixtureNote)), cancellationToken);

        var readyCatalog = await SendIdAsync(new CreateCatalogVersionCommand(documentId, DemoFixtureDefinition.FixtureNote), cancellationToken);
        foreach (var group in DemoFixtureDefinition.Requirements.GroupBy(requirement => requirement.Topic))
        {
            var root = await SendIdAsync(new AddDocumentElementCommand(readyCatalog, null, group.Key, null, DemoFixtureDefinition.FixtureNote), cancellationToken);
            foreach (var requirement in group)
            {
                var element = await SendIdAsync(new AddDocumentElementCommand(readyCatalog, root, requirement.Reference, requirement.Requirement, DemoFixtureDefinition.FixtureNote), cancellationToken);
                await SendIdAsync(new AddQuestionCommand(element, requirement.Question, "Referenzfall Serverraum", requirement.Evidence is null ? null : "Prüfnotiz oder vergleichbarer Nachweis", DemoFixtureDefinition.FixtureNote, [technicalArea]), cancellationToken);
                await RequireSuccessAsync(new SetDocumentElementWeightCommand(element, 4), cancellationToken);
            }
        }

        await RequireSuccessAsync(new SetCatalogReadyCommand(readyCatalog, await CatalogVersionAsync(readyCatalog, cancellationToken), true), cancellationToken);
        var draftCatalog = await SendIdAsync(new CreateCatalogVersionCommand(documentId, DemoFixtureDefinition.FixtureNote), cancellationToken);
        await SendIdAsync(new AddDocumentElementCommand(draftCatalog, null, "Redaktioneller Entwurf", "Dieser DRAFT-Katalog bleibt gezielt editierbar.", DemoFixtureDefinition.FixtureNote), cancellationToken);

        var input = new AuditInput(DemoFixtureDefinition.InitialAuditName, "Finalisierte Referenzprüfung.", serverRoom, readyCatalog, DemoFixtureDefinition.ResponsePolicies, DemoFixtureDefinition.FixtureNote);
        var initial = await SendIdAsync(new CreateAuditCommand(input), cancellationToken);
        await PublishAsync(initial, cancellationToken);
        await RequireSuccessAsync(new ClaimAuditCommand(initial, await AuditVersionAsync(initial, cancellationToken)), cancellationToken);
        await AnswerAllAndFinalizeAsync(initial, cancellationToken);

        var repeat = await SendIdAsync(new CreateAuditRepeatCommand(initial, input with { Name = DemoFixtureDefinition.RepeatAuditName, Notes = DemoFixtureDefinition.FixtureNote }), cancellationToken);
        await PublishAsync(repeat, cancellationToken);

        await SendIdAsync(new CreateAuditCommand(input with { Name = DemoFixtureDefinition.DraftAuditName, Notes = DemoFixtureDefinition.FixtureNote }), cancellationToken);

        var inProgress = await SendIdAsync(new CreateAuditCommand(input with { Name = DemoFixtureDefinition.InProgressAuditName, Notes = DemoFixtureDefinition.FixtureNote }), cancellationToken);
        await PublishAsync(inProgress, cancellationToken);
        await RequireSuccessAsync(new AssignAuditorCommand(inProgress, await DefaultAdministratorIdAsync(cancellationToken), await AuditVersionAsync(inProgress, cancellationToken)), cancellationToken);
        var inProgressQuestions = await QuestionsForAuditAsync(inProgress, cancellationToken);
        var firstQuestion = inProgressQuestions.OrderBy(question => question.AuditQuestionId).First();
        await RequireSuccessAsync(new AnswerAuditQuestionCommand(inProgress, firstQuestion.AuditQuestionId, AuditQuestionResult.Yes, null, null, firstQuestion.ConcurrencyVersion), cancellationToken);

        var canceled = await SendIdAsync(new CreateAuditCommand(input with { Name = DemoFixtureDefinition.CanceledAuditName, Notes = DemoFixtureDefinition.FixtureNote }), cancellationToken);
        await PublishAsync(canceled, cancellationToken);
        await RequireSuccessAsync(new CancelAuditCommand(canceled, "Referenz für abgebrochene Prüfungen.", await AuditVersionAsync(canceled, cancellationToken)), cancellationToken);

        var marker = await db.Documents.SingleAsync(document => document.DocumentId == documentId, cancellationToken);
        await RequireSuccessAsync(new UpdateDocumentCommand(documentId, new(
            DemoFixtureDefinition.CompleteTitle,
            "Auditarium",
            $"Demo fixture v{DemoFixtureDefinition.FixtureVersion}",
            null,
            DemoFixtureDefinition.MarkerSource,
            DocumentUsageState.Active,
            null,
            DemoFixtureDefinition.CompleteMarker), marker.ConcurrencyVersion), cancellationToken);

        return new(DemoFixtureApplyStatus.Applied, DemoFixtureDefinition.FixtureVersion);
    }

    private async Task AnswerAllAndFinalizeAsync(long auditId, CancellationToken cancellationToken)
    {
        var questions = await QuestionsForAuditAsync(auditId, cancellationToken);
        foreach (var question in questions)
        {
            var definition = DemoFixtureDefinition.Requirements.Single(requirement => requirement.Reference == question.Reference);
            await RequireSuccessAsync(new AnswerAuditQuestionCommand(auditId, question.AuditQuestionId, definition.InitialResult, definition.Comment, definition.Evidence, question.ConcurrencyVersion), cancellationToken);
        }
        await RequireSuccessAsync(new FinalizeAuditCommand(auditId, await AuditVersionAsync(auditId, cancellationToken)), cancellationToken);
    }

    private Task<List<FixtureQuestion>> QuestionsForAuditAsync(long auditId, CancellationToken cancellationToken) =>
        (from question in db.AuditQuestions
         join auditElement in db.AuditDocumentElements on question.AuditDocumentElementId equals auditElement.AuditDocumentElementId
         join element in db.DocumentElements on auditElement.ElementId equals element.ElementId
         where auditElement.AuditId == auditId
         select new FixtureQuestion(question.AuditQuestionId, question.ConcurrencyVersion, element.Title!)).ToListAsync(cancellationToken);

    private async Task PublishAsync(long auditId, CancellationToken cancellationToken) =>
        await RequireSuccessAsync(new PublishAuditCommand(auditId, await AuditVersionAsync(auditId, cancellationToken)), cancellationToken);

    private async Task<long> ScopeTypeIdAsync(string key, CancellationToken cancellationToken) =>
        await db.ScopeTypes.Where(scope => scope.Key == key).Select(scope => scope.ScopeTypeId).SingleAsync(cancellationToken);

    private async Task<long> DefaultAdministratorIdAsync(CancellationToken cancellationToken) =>
        await db.Users.Where(user => user.UserKey == "DEFAULT_ADMIN").Select(user => user.UserId).SingleAsync(cancellationToken);

    private async Task<long> AuditVersionAsync(long auditId, CancellationToken cancellationToken) =>
        await db.Audits.Where(audit => audit.AuditId == auditId).Select(audit => audit.ConcurrencyVersion).SingleAsync(cancellationToken);

    private async Task<long> CatalogVersionAsync(long catalogVersionId, CancellationToken cancellationToken) =>
        await db.CatalogVersions.Where(catalog => catalog.CatalogVersionId == catalogVersionId).Select(catalog => catalog.ConcurrencyVersion).SingleAsync(cancellationToken);

    private async Task<long> SendIdAsync<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : IRequest<Result<long>>
    {
        var result = await mediator.Send(message, cancellationToken);
        if (!result.IsSuccess) throw Failure(result.Errors);
        return result.Value;
    }

    private async Task RequireSuccessAsync<TMessage>(TMessage message, CancellationToken cancellationToken) where TMessage : IRequest<Result>
    {
        var result = await mediator.Send(message, cancellationToken);
        if (!result.IsSuccess) throw Failure(result.Errors);
    }

    private async Task<FixturePreflight> InspectAsync(CancellationToken cancellationToken)
    {
        var markers = await db.Documents.Where(document => document.Source == DemoFixtureDefinition.MarkerSource).ToListAsync(cancellationToken);
        if (markers.Count > 0)
        {
            if (markers.Count != 1 || markers[0].Notes != DemoFixtureDefinition.CompleteMarker) return FixturePreflight.Incomplete;
            if (await HasForeignBusinessDataAsync(markers[0].DocumentId, cancellationToken)) return FixturePreflight.UnknownBusinessData;
            return await HasCompleteFixtureShapeAsync(markers[0], cancellationToken) ? FixturePreflight.Complete : FixturePreflight.Incomplete;
        }
        return await HasBusinessDataAsync(cancellationToken) ? FixturePreflight.UnknownBusinessData : FixturePreflight.Empty;
    }

    private async Task<bool> HasCompleteFixtureShapeAsync(Document marker, CancellationToken cancellationToken)
    {
        if (marker.Title != DemoFixtureDefinition.CompleteTitle || await db.Documents.CountAsync(cancellationToken) != 1 || await db.AuditUnits.CountAsync(cancellationToken) != 5 || await db.Audits.CountAsync(cancellationToken) != 5 || !await db.AuditUnits.AllAsync(unit => unit.Notes == DemoFixtureDefinition.FixtureNote, cancellationToken))
            return false;
        var catalogs = await db.CatalogVersions.Where(catalog => catalog.DocumentId == marker.DocumentId).OrderBy(catalog => catalog.VersionNumber).ToListAsync(cancellationToken);
        if (catalogs.Count != 2 || catalogs[0].CatalogState != CatalogState.Ready || catalogs[1].CatalogState != CatalogState.Draft || catalogs.Any(catalog => catalog.Notes != DemoFixtureDefinition.FixtureNote)) return false;
        var catalogIds = catalogs.Select(catalog => catalog.CatalogVersionId).ToArray();
        var elements = db.DocumentElements.Where(element => catalogIds.Contains(element.CatalogVersionId));
        if (await elements.CountAsync(cancellationToken) != 9 || !await elements.AllAsync(element => element.Notes == DemoFixtureDefinition.FixtureNote, cancellationToken) || await db.Questions.Where(question => elements.Select(element => element.ElementId).Contains(question.ElementId)).CountAsync(cancellationToken) != 4 || !await db.Questions.Where(question => elements.Select(element => element.ElementId).Contains(question.ElementId)).AllAsync(question => question.Notes == DemoFixtureDefinition.FixtureNote, cancellationToken))
            return false;
        var audits = await db.Audits.ToListAsync(cancellationToken);
        return audits.All(audit => audit.Notes == DemoFixtureDefinition.FixtureNote) && audits.Count(audit => audit.AuditState == AuditState.Finalized) == 1 && audits.Any(audit => audit.Name == DemoFixtureDefinition.RepeatAuditName && audit.OriginAuditId is not null) && audits.Any(audit => audit.AuditState == AuditState.Draft) && audits.Any(audit => audit.AuditState == AuditState.Ready) && audits.Any(audit => audit.AuditState == AuditState.InProgress) && audits.Any(audit => audit.AuditState == AuditState.Canceled);
    }

    private async Task<bool> HasBusinessDataAsync(CancellationToken cancellationToken) =>
        await db.Documents.AnyAsync(cancellationToken) || await db.CatalogVersions.AnyAsync(cancellationToken) || await db.DocumentElements.AnyAsync(cancellationToken) || await db.Questions.AnyAsync(cancellationToken) || await db.AuditUnits.AnyAsync(cancellationToken) || await db.Audits.AnyAsync(cancellationToken) || await db.FileItems.AnyAsync(cancellationToken);

    private async Task<bool> HasForeignBusinessDataAsync(long markerDocumentId, CancellationToken cancellationToken) =>
        await db.Documents.AnyAsync(document => document.DocumentId != markerDocumentId, cancellationToken) ||
        await db.AuditUnits.AnyAsync(unit => unit.Notes != DemoFixtureDefinition.FixtureNote, cancellationToken) ||
        await db.Audits.AnyAsync(audit => audit.Notes != DemoFixtureDefinition.FixtureNote, cancellationToken) ||
        await db.FileItems.AnyAsync(cancellationToken);

    private static void ThrowIfPreflightRejected(FixturePreflight preflight)
    {
        if (preflight == FixturePreflight.Incomplete)
            throw new DemoDataFixtureException("Demo fixture v1 is incomplete. Reset the Development database externally before trying again.");
        if (preflight == FixturePreflight.UnknownBusinessData)
            throw new DemoDataFixtureException("Unknown business data exists. DemoData will not modify it; reset or use an empty Development database.");
    }

    private static DemoDataFixtureException Failure(IReadOnlyList<AppError> errors) => new($"Demo fixture creation failed: {string.Join(", ", errors.Select(error => error.Code))}");

    private sealed record FixtureQuestion(long AuditQuestionId, long ConcurrencyVersion, string Reference);
    private enum FixturePreflight { Empty, Complete, Incomplete, UnknownBusinessData }
}
