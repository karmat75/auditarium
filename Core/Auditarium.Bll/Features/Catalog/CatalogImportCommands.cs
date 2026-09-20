// SPDX-License-Identifier: MIT
using System.Text.Json;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Catalog;

public enum ImportStatus { Valid, PartiallyValid, Rejected }
public enum ImportApplyMode { Full, Partial }

public sealed record ImportIssue(string Code, string Path, string Message);
public sealed record ImportCandidate(string ElementId, int ElementCount, int QuestionCount);
public sealed record CatalogImportReport(ImportStatus Status, int RecognizedElementCount, int RecognizedQuestionCount, IReadOnlyList<ImportIssue> Errors, IReadOnlyList<ImportIssue> Warnings, IReadOnlyList<ImportCandidate> ApplicableRoots, IReadOnlyList<string> RejectedElementIds);

[RequiresPermission("Documents.Manage")]
public sealed record ValidateCatalogImportCommand(string PackageJson) : IRequest<Result<CatalogImportReport>>;
[RequiresPermission("Documents.Manage")]
public sealed record ApplyCatalogImportCommand(string PackageJson, ImportApplyMode Mode, IReadOnlyCollection<string> SelectedRootElementIds) : IRequest<Result<CatalogImportReport>>;

/// <summary>Validates untrusted Catalog Import Format v1 input and applies only a user-selected valid subset.</summary>
public sealed class CatalogImportHandler(IAuditariumDbContext db) :
    IRequestHandler<ValidateCatalogImportCommand, Result<CatalogImportReport>>,
    IRequestHandler<ApplyCatalogImportCommand, Result<CatalogImportReport>>
{
    public async ValueTask<Result<CatalogImportReport>> Handle(ValidateCatalogImportCommand message, CancellationToken ct)
        => Result<CatalogImportReport>.Success(await ValidateAsync(message.PackageJson, ct));

    public async ValueTask<Result<CatalogImportReport>> Handle(ApplyCatalogImportCommand message, CancellationToken ct)
    {
        var validation = await BuildPlanAsync(message.PackageJson, ct);
        if (validation.Report.Status == ImportStatus.Rejected)
            return Result<CatalogImportReport>.Failure(new AppError("IMPORT.NOT_APPLICABLE", ErrorType.Validation));
        var package = validation.Package!;

        var candidates = validation.Candidates.ToDictionary(x => x.Id, StringComparer.Ordinal);
        IReadOnlyCollection<string> selected = message.Mode switch
        {
            ImportApplyMode.Full when message.SelectedRootElementIds.Count == 0 => candidates.Keys.ToArray(),
            ImportApplyMode.Full => message.SelectedRootElementIds,
            ImportApplyMode.Partial when message.SelectedRootElementIds.Count > 0 => message.SelectedRootElementIds,
            _ => []
        };
        if (selected.Count == 0 || selected.Distinct(StringComparer.Ordinal).Count() != selected.Count || selected.Any(x => !candidates.ContainsKey(x)))
            return Result<CatalogImportReport>.Failure(new AppError("IMPORT.APPLY_SELECTION_INVALID", ErrorType.Validation));

        var catalog = await db.CatalogVersions.SingleOrDefaultAsync(x => x.CatalogVersionId == package.CatalogVersionId, ct);
        if (catalog is null || catalog.CatalogState != CatalogState.Draft)
            return Result<CatalogImportReport>.Failure(new AppError("IMPORT.TARGET_NOT_DRAFT", ErrorType.Conflict));
        if (catalog.DraftRevision != package.DraftRevision)
            return Result<CatalogImportReport>.Failure(new AppError("IMPORT.BASE_REVISION_MISMATCH", ErrorType.Conflict));

        var chosen = selected.Select(id => candidates[id]).SelectMany(root => Descendants(root, package.Elements)).ToArray();
        var elementById = new Dictionary<string, DocumentElement>(StringComparer.Ordinal);
        foreach (var element in chosen)
        {
            var entity = new DocumentElement
            {
                CatalogVersionId = catalog.CatalogVersionId,
                Title = Null(element.Title),
                Text = Null(element.Text),
                Notes = Null(element.Notes),
                SortOrder = element.SortOrder
            };
            elementById.Add(element.Id, entity);
            db.DocumentElements.Add(entity);
        }
        foreach (var element in chosen.Where(x => x.ParentId is not null))
            elementById[element.Id].ParentElement = elementById[element.ParentId!];

        var scopeIds = await db.ScopeTypes.ToDictionaryAsync(x => x.Key, x => x.ScopeTypeId, StringComparer.Ordinal, ct);
        foreach (var element in chosen)
        {
            foreach (var question in element.Questions.Where(x => x.IsValid))
            {
                var entity = new Question
                {
                    Element = elementById[element.Id],
                    SortOrder = question.SortOrder,
                    Text = question.Text!.Trim(),
                    VerificationHint = Null(question.VerificationHint),
                    EvidenceHint = Null(question.EvidenceHint),
                    Notes = Null(question.Notes)
                };
                db.Questions.Add(entity);
                foreach (var scopeKey in question.ScopeKeys!)
                    db.QuestionScopeTypes.Add(new QuestionScopeType { Question = entity, ScopeTypeId = scopeIds[scopeKey] });
            }
            if (element.Weight is not null && element.Questions.Any(x => x.IsValid))
                db.DocumentElementWeights.Add(new DocumentElementWeight { Element = elementById[element.Id], Weight = element.Weight.Value });
        }

        // The catalog concurrency token makes the revision check effective until commit; SaveChanges writes the complete graph atomically.
        catalog.DraftRevision++;
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Result<CatalogImportReport>.Failure(new AppError("IMPORT.BASE_REVISION_MISMATCH", ErrorType.Conflict)); }
        return Result<CatalogImportReport>.Success(validation.Report);
    }

    private async Task<CatalogImportReport> ValidateAsync(string json, CancellationToken ct) => (await BuildPlanAsync(json, ct)).Report;

    private async Task<Plan> BuildPlanAsync(string json, CancellationToken ct)
    {
        var errors = new List<ImportIssue>(); var warnings = new List<ImportIssue>();
        var package = Parse(json, errors);
        if (package is null) return Plan.Rejected(errors, warnings);
        if (package.ImportFormatVersion != 1) errors.Add(Error("IMPORT.FORMAT_VERSION_UNKNOWN", "$.import_format_version", "Only import format version 1 is supported."));
        if (package.CatalogVersionId < 1) errors.Add(Error("IMPORT.CATALOG_VERSION_INVALID", "$.catalog_version_id", "catalog_version_id must be positive."));
        if (package.DraftRevision < 1) errors.Add(Error("IMPORT.DRAFT_REVISION_INVALID", "$.draft_revision", "draft_revision must be positive."));
        if (errors.Count > 0) return Plan.Rejected(errors, warnings);

        var catalog = await db.CatalogVersions.AsNoTracking().SingleOrDefaultAsync(x => x.CatalogVersionId == package.CatalogVersionId, ct);
        if (catalog is null) errors.Add(Error("IMPORT.CATALOG_NOT_FOUND", "$.catalog_version_id", "The target catalog version does not exist."));
        else if (catalog.CatalogState != CatalogState.Draft) errors.Add(Error("IMPORT.TARGET_NOT_DRAFT", "$.catalog_version_id", "The target catalog version is not a DRAFT."));
        else if (catalog.DraftRevision != package.DraftRevision) errors.Add(Error("IMPORT.BASE_REVISION_MISMATCH", "$.draft_revision", "The package is based on an older DRAFT revision."));
        if (errors.Count > 0) return Plan.Rejected(errors, warnings);

        var scopeKeys = await db.ScopeTypes.AsNoTracking().Select(x => x.Key).ToHashSetAsync(StringComparer.Ordinal, ct);
        ValidateElements(package.Elements, scopeKeys, errors, warnings);
        var existingRootSortOrders = await db.DocumentElements.AsNoTracking().Where(x => x.CatalogVersionId == package.CatalogVersionId && x.ParentElementId == null).Select(x => x.SortOrder).ToHashSetAsync(ct);
        foreach (var root in package.Elements.Where(x => x.ParentId is null && existingRootSortOrders.Contains(x.SortOrder)))
        {
            errors.Add(Error("IMPORT.SORT_ORDER_CONFLICT", $"{root.Path}.sort_order", "sort_order conflicts with an existing DRAFT root element."));
            root.Invalid = true;
        }
        var invalid = PropagateInvalidity(package.Elements);
        var candidates = package.Elements.Where(x => x.ParentId is null && !invalid.Contains(x.Id)).ToArray();
        var report = new CatalogImportReport(errors.Count == 0 ? ImportStatus.Valid : candidates.Length == 0 ? ImportStatus.Rejected : ImportStatus.PartiallyValid,
            package.Elements.Count, package.Elements.Sum(x => x.Questions.Count), errors, warnings,
            candidates.Select(x => new ImportCandidate(x.Id, Descendants(x, package.Elements).Count(), Descendants(x, package.Elements).Sum(e => e.Questions.Count(q => q.IsValid)))).ToArray(), invalid.OrderBy(x => x, StringComparer.Ordinal).ToArray());
        return new(package, report, candidates);
    }

    private static Package? Parse(string json, List<ImportIssue> errors)
    {
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { AllowTrailingCommas = false, CommentHandling = JsonCommentHandling.Disallow, MaxDepth = 64 });
            if (document.RootElement.ValueKind != JsonValueKind.Object) { errors.Add(Error("IMPORT.SCHEMA_INVALID", "$", "The package root must be an object.")); return null; }
            RequireProperties(document.RootElement, ["import_format_version", "catalog_version_id", "draft_revision", "elements"], "$", errors);
            RejectUnknownProperties(document.RootElement, ["import_format_version", "catalog_version_id", "draft_revision", "elements"], "$", errors);
            if (errors.Count > 0) return null;
            var elements = ParseElements(document.RootElement.GetProperty("elements"), "$.elements", errors);
            return new(document.RootElement.GetProperty("import_format_version").GetInt32(), document.RootElement.GetProperty("catalog_version_id").GetInt64(), document.RootElement.GetProperty("draft_revision").GetInt32(), elements);
        }
        catch (JsonException) { errors.Add(Error("IMPORT.JSON_INVALID", "$", "The package is not valid JSON.")); return null; }
        catch (InvalidOperationException) { errors.Add(Error("IMPORT.SCHEMA_INVALID", "$", "A required value has an invalid JSON type.")); return null; }
    }

    private static List<Element> ParseElements(JsonElement value, string path, List<ImportIssue> errors)
    {
        if (value.ValueKind != JsonValueKind.Array) { errors.Add(Error("IMPORT.SCHEMA_INVALID", path, "elements must be an array.")); return []; }
        var result = new List<Element>();
        for (var index = 0; index < value.GetArrayLength(); index++)
        {
            var item = value[index]; var itemPath = $"{path}[{index}]";
            if (item.ValueKind != JsonValueKind.Object) { errors.Add(Error("IMPORT.SCHEMA_INVALID", itemPath, "An element must be an object.")); continue; }
            RequireProperties(item, ["id", "parent_id", "sort_order", "title", "text", "notes", "weight", "questions"], itemPath, errors);
            RejectUnknownProperties(item, ["id", "parent_id", "sort_order", "title", "text", "notes", "weight", "questions"], itemPath, errors);
            if (!HasProperties(item, ["id", "parent_id", "sort_order", "title", "text", "notes", "weight", "questions"])) continue;
            var questions = ParseQuestions(item.GetProperty("questions"), $"{itemPath}.questions", errors);
            result.Add(new(ReadString(item.GetProperty("id")), ReadNullableString(item.GetProperty("parent_id")), item.GetProperty("sort_order").GetInt32(), ReadNullableString(item.GetProperty("title")), ReadNullableString(item.GetProperty("text")), ReadNullableString(item.GetProperty("notes")), ReadNullableInt(item.GetProperty("weight")), questions, itemPath));
        }
        return result;
    }

    private static List<QuestionImport> ParseQuestions(JsonElement value, string path, List<ImportIssue> errors)
    {
        if (value.ValueKind != JsonValueKind.Array) { errors.Add(Error("IMPORT.SCHEMA_INVALID", path, "questions must be an array.")); return []; }
        var result = new List<QuestionImport>();
        for (var index = 0; index < value.GetArrayLength(); index++)
        {
            var item = value[index]; var itemPath = $"{path}[{index}]";
            if (item.ValueKind != JsonValueKind.Object) { errors.Add(Error("IMPORT.SCHEMA_INVALID", itemPath, "A question must be an object.")); continue; }
            RequireProperties(item, ["sort_order", "text", "verification_hint", "evidence_hint", "notes", "scope_keys"], itemPath, errors);
            RejectUnknownProperties(item, ["sort_order", "text", "verification_hint", "evidence_hint", "notes", "scope_keys"], itemPath, errors);
            if (!HasProperties(item, ["sort_order", "text", "verification_hint", "evidence_hint", "notes", "scope_keys"])) continue;
            var keys = item.GetProperty("scope_keys");
            result.Add(new(item.GetProperty("sort_order").GetInt32(), ReadNullableString(item.GetProperty("text")), ReadNullableString(item.GetProperty("verification_hint")), ReadNullableString(item.GetProperty("evidence_hint")), ReadNullableString(item.GetProperty("notes")), keys.ValueKind == JsonValueKind.Array ? keys.EnumerateArray().Select(ReadString).ToArray() : null, itemPath));
        }
        return result;
    }

    private static void ValidateElements(IReadOnlyList<Element> elements, IReadOnlySet<string> scopeKeys, List<ImportIssue> errors, List<ImportIssue> warnings)
    {
        var byId = new Dictionary<string, Element>(StringComparer.Ordinal);
        foreach (var element in elements)
        {
            if (string.IsNullOrWhiteSpace(element.Id)) { errors.Add(Error("IMPORT.ELEMENT_ID_INVALID", $"{element.Path}.id", "Element id is required.")); element.Invalid = true; }
            else if (!byId.TryAdd(element.Id, element)) { errors.Add(Error("IMPORT.ELEMENT_ID_DUPLICATE", $"{element.Path}.id", "Element ids must be unique.")); byId[element.Id].Invalid = true; element.Invalid = true; }
            if (element.SortOrder < 0) { errors.Add(Error("IMPORT.SORT_ORDER_INVALID", $"{element.Path}.sort_order", "sort_order must not be negative.")); element.Invalid = true; }
            if (Empty(element.Title) && Empty(element.Text)) { errors.Add(Error("IMPORT.ELEMENT_CONTENT_REQUIRED", element.Path, "An element requires title or text.")); element.Invalid = true; }
            if (element.Weight is not null && element.Weight is not (1 or 2 or 4 or 5)) { errors.Add(Error("IMPORT.WEIGHT_INVALID", $"{element.Path}.weight", "weight must be 1, 2, 4, or 5.")); element.Invalid = true; }
            foreach (var question in element.Questions) ValidateQuestion(question, scopeKeys, errors, warnings);
            foreach (var group in element.Questions.GroupBy(x => x.SortOrder).Where(x => x.Count() > 1)) foreach (var question in group)
            {
                errors.Add(Error("IMPORT.QUESTION_SORT_ORDER_DUPLICATE", $"{question.Path}.sort_order", "Question sort_order values must be unique per element."));
                question.IsValid = false;
            }
        }
        foreach (var element in elements)
        {
            if (element.ParentId is not null && (!byId.ContainsKey(element.ParentId) || element.ParentId == element.Id)) { errors.Add(Error("IMPORT.PARENT_INVALID", $"{element.Path}.parent_id", "parent_id must reference another imported element.")); element.Invalid = true; }
            if (element.Questions.Any(x => x.IsValid) && Empty(element.Text)) { errors.Add(Error("IMPORT.QUESTIONED_ELEMENT_TEXT_REQUIRED", element.Path, "An element with imported questions requires text.")); element.Invalid = true; }
            if (element.Weight is not null && !element.Questions.Any(x => x.IsValid)) { errors.Add(Error("IMPORT.WEIGHT_REQUIRES_QUESTION", $"{element.Path}.weight", "weight requires an imported valid question.")); element.Invalid = true; }
        }
        foreach (var group in elements.GroupBy(x => (x.ParentId, x.SortOrder))) if (group.Count() > 1) foreach (var element in group) { errors.Add(Error("IMPORT.SORT_ORDER_DUPLICATE", $"{element.Path}.sort_order", "Sibling sort_order values must be unique.")); element.Invalid = true; }
        foreach (var element in elements) if (HasCycle(element, byId)) { errors.Add(Error("IMPORT.HIERARCHY_CYCLE", element.Path, "Element hierarchy must not contain a cycle.")); element.Invalid = true; }
    }

    private static void ValidateQuestion(QuestionImport question, IReadOnlySet<string> scopeKeys, List<ImportIssue> errors, List<ImportIssue> warnings)
    {
        if (question.SortOrder < 0) { errors.Add(Error("IMPORT.QUESTION_SORT_ORDER_INVALID", $"{question.Path}.sort_order", "sort_order must not be negative.")); question.IsValid = false; }
        if (Empty(question.Text)) { errors.Add(Error("IMPORT.QUESTION_TEXT_REQUIRED", $"{question.Path}.text", "Question text is required.")); question.IsValid = false; }
        if (question.ScopeKeys is null || question.ScopeKeys.Length == 0 || question.ScopeKeys.Distinct(StringComparer.Ordinal).Count() != question.ScopeKeys?.Length || question.ScopeKeys.Any(x => !scopeKeys.Contains(x))) { errors.Add(Error("IMPORT.SCOPE_INVALID", $"{question.Path}.scope_keys", "scope_keys must contain distinct known scope keys.")); question.IsValid = false; }
        if (Empty(question.VerificationHint)) warnings.Add(new("IMPORT.VERIFICATION_HINT_MISSING", $"{question.Path}.verification_hint", "Verification hint is empty."));
    }

    private static HashSet<string> PropagateInvalidity(IReadOnlyList<Element> elements)
    {
        var invalid = elements.Where(x => x.Invalid).Select(x => x.Id).ToHashSet(StringComparer.Ordinal); var changed = true;
        while (changed) { changed = false; foreach (var element in elements.Where(x => x.ParentId is not null && invalid.Contains(x.ParentId))) changed |= invalid.Add(element.Id); }
        return invalid;
    }
    private static bool HasCycle(Element element, IReadOnlyDictionary<string, Element> byId) { var seen = new HashSet<string>(StringComparer.Ordinal); for (var id = element.Id; byId.TryGetValue(id, out var current) && current.ParentId is not null; id = current.ParentId) if (!seen.Add(id)) return true; return false; }
    private static IEnumerable<Element> Descendants(Element root, IReadOnlyList<Element> all)
    {
        yield return root;
        foreach (var child in all.Where(x => x.ParentId == root.Id)) foreach (var descendant in Descendants(child, all)) yield return descendant;
    }
    private static bool Empty(string? value) => string.IsNullOrWhiteSpace(value);
    private static string? Null(string? value) => Empty(value) ? null : value!.Trim();
    private static string ReadString(JsonElement value) => value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : throw new InvalidOperationException();
    private static string? ReadNullableString(JsonElement value) => value.ValueKind == JsonValueKind.Null ? null : ReadString(value);
    private static int? ReadNullableInt(JsonElement value) => value.ValueKind == JsonValueKind.Null ? null : value.ValueKind == JsonValueKind.Number ? value.GetInt32() : throw new InvalidOperationException();
    private static void RequireProperties(JsonElement value, IEnumerable<string> names, string path, List<ImportIssue> errors) { foreach (var name in names) if (!value.TryGetProperty(name, out _)) errors.Add(Error("IMPORT.SCHEMA_REQUIRED", path, $"Required property '{name}' is missing.")); }
    private static bool HasProperties(JsonElement value, IEnumerable<string> names) => names.All(name => value.TryGetProperty(name, out _));
    private static void RejectUnknownProperties(JsonElement value, IEnumerable<string> names, string path, List<ImportIssue> errors) { foreach (var property in value.EnumerateObject()) if (!names.Contains(property.Name, StringComparer.Ordinal)) errors.Add(Error("IMPORT.SCHEMA_UNKNOWN_PROPERTY", path, $"Property '{property.Name}' is not supported.")); }
    private static ImportIssue Error(string code, string path, string message) => new(code, path, message);

    private sealed record Package(int ImportFormatVersion, long CatalogVersionId, int DraftRevision, List<Element> Elements);
    private sealed class Element(string id, string? parentId, int sortOrder, string? title, string? text, string? notes, int? weight, List<QuestionImport> questions, string path)
    {
        public string Id { get; } = id; public string? ParentId { get; } = parentId; public int SortOrder { get; } = sortOrder; public string? Title { get; } = title; public string? Text { get; } = text; public string? Notes { get; } = notes; public int? Weight { get; } = weight; public List<QuestionImport> Questions { get; } = questions; public string Path { get; } = path; public bool Invalid { get; set; }
    }
    private sealed class QuestionImport(int sortOrder, string? text, string? verificationHint, string? evidenceHint, string? notes, string[]? scopeKeys, string path)
    {
        public int SortOrder { get; } = sortOrder; public string? Text { get; } = text; public string? VerificationHint { get; } = verificationHint; public string? EvidenceHint { get; } = evidenceHint; public string? Notes { get; } = notes; public string[]? ScopeKeys { get; } = scopeKeys; public string Path { get; } = path; public bool IsValid { get; set; } = true;
    }
    private sealed record Plan(Package? Package, CatalogImportReport Report, IReadOnlyList<Element> Candidates)
    {
        public static Plan Rejected(List<ImportIssue> errors, List<ImportIssue> warnings) => new(null, new(ImportStatus.Rejected, 0, 0, errors, warnings, [], []), []);
    }
}
