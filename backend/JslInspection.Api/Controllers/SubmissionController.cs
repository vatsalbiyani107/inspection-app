using JslInspection.Api.Data;
using JslInspection.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JslInspection.Api.Controllers;

/// <summary>
/// Handles checklist submissions (Inspector flow).
/// Updated flow:
/// 1) POST /api/Submission  -> creates Draft submission + items, returns itemId map
/// 2) POST /api/Submission/{id}/finalize -> validates photo rules, sets PendingSupervisor, creates EmailOutbox
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SubmissionController : ControllerBase
{
    private readonly AppDbContext _db;

    public SubmissionController(AppDbContext db)
    {
        _db = db;
    }

    // ----------------------------
    // Request DTOs
    // ----------------------------

    public record SubmissionItemRequest(
        int ChecklistItemTemplateId,
        string? ValueText,
        decimal? ValueNumber,
        string? Comment,
        bool IsNotOk
    );

    public record CreateSubmissionRequest(
        int ChecklistTemplateId,
        string SubmittedBy,
        string? OverallRemark,
        List<SubmissionItemRequest> Items
    );

    /// <summary>
    /// STEP 1: Create Draft submission + items.
    /// Returns:
    /// - submissionId
    /// - itemMap: templateItemId -> submissionItemId
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubmissionRequest req)
    {
        if (req.ChecklistTemplateId <= 0)
            return BadRequest("ChecklistTemplateId is required.");

        if (string.IsNullOrWhiteSpace(req.SubmittedBy))
            return BadRequest("SubmittedBy is required.");

        if (req.Items == null || req.Items.Count == 0)
            return BadRequest("Items are required.");

        // 1) Load template
        var template = await _db.ChecklistTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == req.ChecklistTemplateId && t.IsActive);

        if (template == null)
            return NotFound("Checklist template not found (or inactive).");

        // 2) Load template items (rules)
        var templateItems = await _db.ChecklistItemTemplates
            .AsNoTracking()
            .Where(i => i.ChecklistTemplateId == req.ChecklistTemplateId)
            .ToListAsync();

        var templateById = templateItems.ToDictionary(x => x.Id, x => x);

        // 3) Validate: each submitted item must exist in template
        foreach (var it in req.Items)
        {
            if (!templateById.ContainsKey(it.ChecklistItemTemplateId))
                return BadRequest($"Item {it.ChecklistItemTemplateId} does not belong to this checklist template.");
        }

        // 4) Validate required items filled
        foreach (var ti in templateItems)
        {
            if (!ti.IsRequired) continue;

            var submitted = req.Items.FirstOrDefault(x => x.ChecklistItemTemplateId == ti.Id);
            if (submitted == null)
                return BadRequest($"Required item missing: {ti.ItemText}");

            var hasText = !string.IsNullOrWhiteSpace(submitted.ValueText);
            var hasNumber = submitted.ValueNumber.HasValue;

            if (!hasText && !hasNumber)
                return BadRequest($"Required item not filled: {ti.ItemText}");
        }

        // 5) Create Submission as Draft (photos can be uploaded after)
        var submission = new ChecklistSubmission
        {
            ChecklistTemplateId = req.ChecklistTemplateId,
            SubmittedBy = req.SubmittedBy.Trim(),
            OverallRemark = string.IsNullOrWhiteSpace(req.OverallRemark) ? null : req.OverallRemark.Trim(),
            Status = "Draft",
            SubmittedAtUtc = DateTimeOffset.UtcNow
        };

        _db.ChecklistSubmissions.Add(submission);
        await _db.SaveChangesAsync(); // now submission.Id exists

        // 6) Create all item rows (single SaveChanges at end)
        var itemRows = new List<ChecklistSubmissionItem>();

        foreach (var it in req.Items)
        {
            itemRows.Add(new ChecklistSubmissionItem
            {
                ChecklistSubmissionId = submission.Id,
                ChecklistItemTemplateId = it.ChecklistItemTemplateId,
                ValueText = string.IsNullOrWhiteSpace(it.ValueText) ? null : it.ValueText.Trim(),
                ValueNumber = it.ValueNumber,
                Comment = string.IsNullOrWhiteSpace(it.Comment) ? null : it.Comment.Trim(),
                IsNotOk = it.IsNotOk
            });
        }

        _db.ChecklistSubmissionItems.AddRange(itemRows);
        await _db.SaveChangesAsync(); // now itemRows have Ids

        // 7) Build mapping templateItemId -> submissionItemId
        var itemMap = itemRows.Select(r => new
        {
            checklistItemTemplateId = r.ChecklistItemTemplateId,
            submissionItemId = r.Id
        }).ToList();

        return Ok(new
        {
            submissionId = submission.Id,
            status = submission.Status,
            itemMap
        });
    }

    /// <summary>
    /// STEP 2: Finalize submission after photos are uploaded.
    /// Validates:
    /// - If item is Not OK and template requires photo -> must have at least 1 SubmissionPhoto row.
    /// Then:
    /// - sets status to PendingSupervisor
    /// - creates EmailOutbox row for supervisor
    /// </summary>
    /// POST /api/Submission/{submissionId}/finalize
    [HttpPost("{submissionId:int}/finalize")]
    public async Task<IActionResult> Finalize(int submissionId)
    {
        var submission = await _db.ChecklistSubmissions
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null)
            return NotFound("Submission not found.");

        if (submission.Status != "Draft")
            return BadRequest($"Cannot finalize because status is '{submission.Status}'.");

        // Load template + rules
        var template = await _db.ChecklistTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == submission.ChecklistTemplateId);

        if (template == null)
            return NotFound("Checklist template not found.");

        var templateItems = await _db.ChecklistItemTemplates
            .AsNoTracking()
            .Where(i => i.ChecklistTemplateId == submission.ChecklistTemplateId)
            .ToListAsync();

        var ruleByTemplateItemId = templateItems.ToDictionary(x => x.Id, x => x);

        // Load submission items
        var items = await _db.ChecklistSubmissionItems
            .AsNoTracking()
            .Where(i => i.ChecklistSubmissionId == submissionId)
            .ToListAsync();

        // Validate photo rule
        foreach (var it in items)
        {
            if (!it.IsNotOk) continue;

            if (ruleByTemplateItemId.TryGetValue(it.ChecklistItemTemplateId, out var rule) && rule.RequiresPhotoIfNotOk)
            {
                var hasPhoto = await _db.SubmissionPhotos
                    .AsNoTracking()
                    .AnyAsync(p => p.ChecklistSubmissionItemId == it.Id);

                if (!hasPhoto)
                    return BadRequest($"Photo is required for Not OK item: {rule.ItemText}");
            }
        }

        // Mark ready for supervisor
        submission.Status = "PendingSupervisor";
        await _db.SaveChangesAsync();

        // Demo email outbox
        _db.EmailOutbox.Add(new EmailOutbox
        {
            ToEmail = "supervisor@jsl.local",
            Subject = $"Inspection Submitted: {template.Name} (Submission #{submission.Id})",
            BodyHtml =
                $"<h3>Inspection Submitted</h3>" +
                $"<p><b>Checklist:</b> {template.Name}</p>" +
                $"<p><b>Submitted By:</b> {submission.SubmittedBy}</p>" +
                $"<p><b>Status:</b> {submission.Status}</p>" +
                $"<p><b>Submission Id:</b> {submission.Id}</p>" +
                $"<p>Please review in Supervisor mode.</p>",
            ChecklistSubmissionId = submission.Id,
            Status = "Pending",
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync();

        return Ok(new
        {
            submissionId = submission.Id,
            status = submission.Status,
            message = "Finalized successfully."
        });
    }
}
