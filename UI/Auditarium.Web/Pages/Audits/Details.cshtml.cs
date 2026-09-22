// SPDX-License-Identifier: MIT
using Auditarium.Bll.Features.Audits;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Auditarium.Web.Pages;

namespace Auditarium.Web.Pages.Audits;

[Authorize]
public sealed class DetailsModel(IMediator mediator, ICurrentActor actor, IPermissionEvaluator permissionEvaluator) : PageModel
{
    [BindProperty] public AuditInputModel Input { get; set; } = new();
    [BindProperty] public PublishAuditInputModel PublishInput { get; set; } = new();
    [BindProperty] public AuditActionInputModel ActionInput { get; set; } = new();
    [BindProperty] public AssignmentInputModel AssignmentInput { get; set; } = new();
    [BindProperty] public ReasonInputModel CancelInput { get; set; } = new();
    [BindProperty] public ReasonInputModel ReopenInput { get; set; } = new();
    [BindProperty] public AnswerAuditQuestionInputModel AnswerInput { get; set; } = new();
    [BindProperty] public SoftDeleteInputModel DeleteInput { get; set; } = new();
    public AuditDetails Audit { get; private set; } = default!;
    public AuditConfigurationOptions Options { get; private set; } = new([], [], []);
    public AuditPreview? Preview { get; private set; }
    public IReadOnlyList<AuditorOption> AuditorOptions { get; private set; } = [];
    public bool CanClaim { get; private set; }
    public bool CanReleaseOwn { get; private set; }
    public bool CanAssign { get; private set; }
    public bool CanAnswer { get; private set; }
    public bool CanFinalize { get; private set; }
    public bool CanCancel { get; private set; }
    public bool CanReopen { get; private set; }
    public bool CanDelete { get; private set; }
    public async Task<IActionResult> OnGetAsync(long id, CancellationToken ct) => await LoadAsync(id, ct);
    public async Task<IActionResult> OnPostAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateAuditDraftCommand(id, Input.ToBll(), Input.ConcurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage(new { id });
        result.ApplyTo(ModelState); return await LoadAsync(id, ct);
    }
    public async Task<IActionResult> OnPostPublishAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new PublishAuditCommand(id, PublishInput.ConcurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage(new { id });
        result.ApplyTo(ModelState); return await LoadAsync(id, ct, reloadInput: true);
    }
    public async Task<IActionResult> OnPostClaimAsync(long id, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new ClaimAuditCommand(id, ActionInput.ConcurrencyVersion), ct), ct);
    public async Task<IActionResult> OnPostReleaseOwnAsync(long id, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new ReleaseOwnAuditCommand(id, ActionInput.ConcurrencyVersion), ct), ct);
    public async Task<IActionResult> OnPostAssignAsync(long id, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new AssignAuditorCommand(id, AssignmentInput.UserId, AssignmentInput.ConcurrencyVersion), ct), ct);
    public async Task<IActionResult> OnPostFinalizeAsync(long id, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new FinalizeAuditCommand(id, ActionInput.ConcurrencyVersion), ct), ct);
    public async Task<IActionResult> OnPostCancelAsync(long id, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new CancelAuditCommand(id, CancelInput.Reason, CancelInput.ConcurrencyVersion), ct), ct);
    public async Task<IActionResult> OnPostReopenAsync(long id, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new ReopenAuditCommand(id, ReopenInput.Reason, ReopenInput.ConcurrencyVersion), ct), ct);
    public async Task<IActionResult> OnPostAnswerAsync(long id, long questionId, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new AnswerAuditQuestionCommand(id, questionId, AnswerInput.Result, AnswerInput.Comment, AnswerInput.Evidence, AnswerInput.ConcurrencyVersion), ct), ct);
    public async Task<IActionResult> OnPostResetAnswerAsync(long id, long questionId, CancellationToken ct) => await ExecuteAsync(id, mediator.Send(new ResetAuditQuestionCommand(id, questionId, ActionInput.ConcurrencyVersion), ct), ct);
    public async Task<IActionResult> OnPostDeleteAsync(long id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteAuditCommand(id, DeleteInput.Reason, DeleteInput.ConcurrencyVersion), ct);
        if (result.IsSuccess) return RedirectToPage("Index");
        result.ApplyTo(ModelState);
        return await LoadAsync(id, ct);
    }

    private async Task<IActionResult> ExecuteAsync(long id, ValueTask<Auditarium.Common.Results.Result> pending, CancellationToken ct)
    {
        var result = await pending;
        if (result.IsSuccess) return RedirectToPage(new { id });
        result.ApplyTo(ModelState);
        return await LoadAsync(id, ct);
    }
    private async Task<IActionResult> LoadAsync(long id, CancellationToken ct, bool reloadInput = false)
    {
        var audit = await mediator.Send(new GetAuditQuery(id), ct);
        if (!audit.IsSuccess) { audit.ApplyTo(ModelState); return Page(); }
        Audit = audit.Value!;
        if (!Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)) AssignmentInput.UserId = Audit.AssignedAuditorUserId;
        await LoadPermissionsAsync(ct);
        if (reloadInput || !Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase)) Input = AuditInputModel.From(Audit);
        PublishInput.ConcurrencyVersion = Audit.ConcurrencyVersion;
        DeleteInput.ConcurrencyVersion = Audit.ConcurrencyVersion;
        if (Audit.AuditState == AuditState.Draft)
        {
            var options = await mediator.Send(new GetAuditConfigurationOptionsQuery(), ct);
            if (!options.IsSuccess) options.ApplyTo(ModelState); else Options = options.Value!;
            var preview = await mediator.Send(new GetAuditPreviewQuery(id), ct);
            if (!preview.IsSuccess) preview.ApplyTo(ModelState); else Preview = preview.Value;
        }
        if (CanAssign)
        {
            var auditors = await mediator.Send(new GetAuditorOptionsQuery(), ct);
            if (!auditors.IsSuccess) auditors.ApplyTo(ModelState); else AuditorOptions = auditors.Value!;
        }
        return Page();
    }

    private async Task LoadPermissionsAsync(CancellationToken ct)
    {
        if (actor.UserId is not { } userId) return;
        var permissions = await permissionEvaluator.GetPermissionsAsync(userId, ct);
        var activeAssignment = Audit.AssignedAuditorUserId == userId && Audit.AuditState is (AuditState.Ready or AuditState.InProgress);
        CanClaim = permissions.Contains("Audits.Claim") && Audit.AssignedAuditorUserId is null && Audit.AuditState is (AuditState.Ready or AuditState.InProgress);
        CanReleaseOwn = permissions.Contains("Audits.ReleaseOwn") && activeAssignment;
        CanAssign = permissions.Contains("Audits.Assign") && Audit.AuditState is (AuditState.Ready or AuditState.InProgress);
        CanAnswer = permissions.Contains("Audits.Answer") && activeAssignment;
        CanFinalize = permissions.Contains("Audits.Finalize") && activeAssignment && Audit.AuditState == AuditState.InProgress;
        CanCancel = permissions.Contains("Audits.Cancel") && Audit.AuditState is (AuditState.Ready or AuditState.InProgress);
        CanReopen = permissions.Contains("Audits.Reopen") && Audit.AuditState == AuditState.Canceled;
        CanDelete = permissions.Contains("Audits.Delete");
    }
}
