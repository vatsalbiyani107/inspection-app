using JslInspection.Api.Data;
using JslInspection.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JslInspection.Api.Controllers;

/// <summary>
/// Supervisor endpoints for demo:
/// - View pending submissions
/// - View submission details (answers + photos)
/// - Approve / Reject
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SupervisorController : ControllerBase
{
    private readonly AppDbContext _db;

    public SupervisorController(AppDbContext db)
    {
        _db = db;
    }

    // ----------------------------
    // 1) Supervisor "Inbox"
    // ----------------------------

    /// <summary>
    /// List submissions that are waiting for supervisor action.
    /// Demo status we use: "PendingSupervisor"
    /// </summary>
    /// GET /api/supervisor/pending
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        // Join submission with template name so supervisor sees what checklist it was
        var rows = await _db.ChecklistSubmissions
            .AsNoTracking()
            .Where(s => s.Status == "PendingSupervisor")
            .OrderByDescending(s => s.SubmittedAtUtc)
            .Select(s => new
            {
                s.Id,
                s.ChecklistTemplateId,
                ChecklistName = s.ChecklistTemplate!.Name,   // navigation property
                s.SubmittedBy,
                s.SubmittedAtUtc,
                s.Status
            })
            .ToListAsync();

        return Ok(rows);
    }

    // ----------------------------
    // 2) Submission Details (for review screen)
    // ----------------------------

    /// <summary>
    /// Gets one submission with:
    /// - checklist template info
    /// - each answered item (value/comment/not ok)
    /// - photos attached to items
    /// </summary>
    /// GET /api/supervisor/submissions/123
    [HttpGet("submissions/{submissionId:int}")]
    public async Task<IActionResult> GetSubmissionDetails(int submissionId)
    {
        // Load submission + template
        var submission = await _db.ChecklistSubmissions
            .AsNoTracking()
            .Include(s => s.ChecklistTemplate)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null)
            return NotFound("Submission not found.");

        // Load answers for this submission
        var answers = await _db.ChecklistSubmissionItems
            .AsNoTracking()
            .Where(i => i.ChecklistSubmissionId == submissionId)
            .ToListAsync();

        // Load item template text so supervisor sees "question text"
        var itemTemplateIds = answers.Select(a => a.ChecklistItemTemplateId).Distinct().ToList();

        var itemTemplates = await _db.ChecklistItemTemplates
            .AsNoTracking()
            .Where(t => itemTemplateIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t);

        // Load photos for these answers
        var answerIds = answers.Select(a => a.Id).ToList();
        var photos = await _db.SubmissionPhotos
            .AsNoTracking()
            .Where(p => answerIds.Contains(p.ChecklistSubmissionItemId))
            .ToListAsync();

        // Build response: each answer includes its photos
        var answerDtos = answers
            .OrderBy(a => itemTemplates.ContainsKey(a.ChecklistItemTemplateId) ? itemTemplates[a.ChecklistItemTemplateId].SeqNo : 9999)
            .Select(a => new
            {
                a.Id,
                a.ChecklistItemTemplateId,
                ItemText = itemTemplates.ContainsKey(a.ChecklistItemTemplateId) ? itemTemplates[a.ChecklistItemTemplateId].ItemText : "(Unknown Item)",
                InputType = itemTemplates.ContainsKey(a.ChecklistItemTemplateId) ? itemTemplates[a.ChecklistItemTemplateId].InputType : "Unknown",
                a.ValueText,
                a.ValueNumber,
                a.Comment,
                a.IsNotOk,
                Photos = photos
                    .Where(p => p.ChecklistSubmissionItemId == a.Id)
                    .Select(p => new { p.Id, p.PhotoPath, p.CapturedAtUtc })
                    .ToList()
            })
            .ToList();

        return Ok(new
        {
            submission.Id,
            submission.Status,
            submission.SubmittedBy,
            submission.SubmittedAtUtc,
            submission.OverallRemark,
            Checklist = new
            {
                submission.ChecklistTemplateId,
                Name = submission.ChecklistTemplate?.Name,
                Description = submission.ChecklistTemplate?.Description
            },
            Items = answerDtos
        });
    }

    // ----------------------------
    // 3) Approve / Reject
    // ----------------------------

    public record SupervisorDecisionRequest(
        string SupervisorName,
        string? SupervisorRemark
    );

    /// <summary>
    /// Approve a submission.
    /// POST /api/supervisor/submissions/{id}/approve
    /// </summary>
    [HttpPost("submissions/{submissionId:int}/approve")]
    public async Task<IActionResult> Approve(int submissionId, [FromBody] SupervisorDecisionRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SupervisorName))
            return BadRequest("SupervisorName is required.");

        var submission = await _db.ChecklistSubmissions.FirstOrDefaultAsync(s => s.Id == submissionId);
        if (submission == null) return NotFound("Submission not found.");

        // Only allow approve if it is waiting
        if (submission.Status != "PendingSupervisor")
            return BadRequest($"Cannot approve because status is '{submission.Status}'.");

        // Demo rule: if any item is Not OK, force supervisor remark (optional but nice)
        var hasNotOk = await _db.ChecklistSubmissionItems
            .AsNoTracking()
            .AnyAsync(i => i.ChecklistSubmissionId == submissionId && i.IsNotOk);

        if (hasNotOk && string.IsNullOrWhiteSpace(req.SupervisorRemark))
            return BadRequest("SupervisorRemark is required because one or more items are Not OK.");

        submission.Status = "Approved";

        // For demo we store supervisor remark into OverallRemark appended.
        // In production, you'd store approval history in a separate table.
        if (!string.IsNullOrWhiteSpace(req.SupervisorRemark))
        {
            var remarkLine = $"[Supervisor: {req.SupervisorName.Trim()}] {req.SupervisorRemark.Trim()}";
            submission.OverallRemark = string.IsNullOrWhiteSpace(submission.OverallRemark)
                ? remarkLine
                : submission.OverallRemark + "\n" + remarkLine;
        }
        // ----------------------------
        // DEMO EMAIL: notify higher authority after supervisor approval
        // ----------------------------
        var checklistName = await _db.ChecklistTemplates
            .AsNoTracking()
            .Where(t => t.Id == submission.ChecklistTemplateId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync() ?? "Checklist";

        _db.EmailOutbox.Add(new EmailOutbox
        {
            ToEmail = "maintenance.head@jsl.local", // demo; later from settings
            Subject = $"Approved Inspection: {checklistName} (Submission #{submission.Id})",
            BodyHtml =
                $"<h3>Inspection Approved</h3>" +
                $"<p><b>Checklist:</b> {checklistName}</p>" +
                $"<p><b>Submission Id:</b> {submission.Id}</p>" +
                $"<p><b>Approved By:</b> {req.SupervisorName}</p>" +
                $"<p><b>Supervisor Remarks:</b> {(string.IsNullOrWhiteSpace(req.SupervisorRemark) ? "-" : req.SupervisorRemark)}</p>" +
                $"<p>Status is now <b>Approved</b>.</p>",
            ChecklistSubmissionId = submission.Id,
            Status = "Pending",
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync();

        return Ok(new { submissionId = submission.Id, status = submission.Status });
    }

    /// <summary>
    /// Reject a submission.
    /// POST /api/supervisor/submissions/{id}/reject
    /// </summary>
    [HttpPost("submissions/{submissionId:int}/reject")]
    public async Task<IActionResult> Reject(int submissionId, [FromBody] SupervisorDecisionRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SupervisorName))
            return BadRequest("SupervisorName is required.");

        // Reject must have reason
        if (string.IsNullOrWhiteSpace(req.SupervisorRemark))
            return BadRequest("SupervisorRemark is required for rejection.");

        var submission = await _db.ChecklistSubmissions.FirstOrDefaultAsync(s => s.Id == submissionId);
        if (submission == null) return NotFound("Submission not found.");

        if (submission.Status != "PendingSupervisor")
            return BadRequest($"Cannot reject because status is '{submission.Status}'.");

        submission.Status = "Rejected";

        var remarkLine = $"[Supervisor: {req.SupervisorName.Trim()}] {req.SupervisorRemark.Trim()}";
        submission.OverallRemark = string.IsNullOrWhiteSpace(submission.OverallRemark)
            ? remarkLine
            : submission.OverallRemark + "\n" + remarkLine;
        var checklistName = await _db.ChecklistTemplates
            .AsNoTracking()
            .Where(t => t.Id == submission.ChecklistTemplateId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync() ?? "Checklist";

        _db.EmailOutbox.Add(new EmailOutbox
        {
            ToEmail = "maintenance.head@jsl.local",
            Subject = $"Rejected Inspection: {checklistName} (Submission #{submission.Id})",
            BodyHtml =
                $"<h3>Inspection Rejected</h3>" +
                $"<p><b>Checklist:</b> {checklistName}</p>" +
                $"<p><b>Submission Id:</b> {submission.Id}</p>" +
                $"<p><b>Rejected By:</b> {req.SupervisorName}</p>" +
                $"<p><b>Reason:</b> {req.SupervisorRemark}</p>" +
                $"<p>Status is now <b>Rejected</b>.</p>",
            ChecklistSubmissionId = submission.Id,
            Status = "Pending",
            CreatedAtUtc = DateTimeOffset.UtcNow
        });


        await _db.SaveChangesAsync();

        return Ok(new { submissionId = submission.Id, status = submission.Status });
    }
}


