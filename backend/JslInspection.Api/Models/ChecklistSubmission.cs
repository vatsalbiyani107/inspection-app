using System.ComponentModel.DataAnnotations;

namespace JslInspection.Api.Models;

/// <summary>
/// One checklist submission done by an inspector.
/// Example: Inspector fills "CRM Daily Inspection" and presses Submit.
/// This becomes 1 row in ChecklistSubmissions table.
/// </summary>
public class ChecklistSubmission
{
    public int Id { get; set; }

    /// <summary>
    /// Which checklist template was used (e.g., CRM Daily Inspection).
    /// </summary>
    public int ChecklistTemplateId { get; set; }
    public ChecklistTemplate? ChecklistTemplate { get; set; }

    /// <summary>
    /// For demo we store inspector name as text.
    /// Later this becomes a UserId from login.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SubmittedBy { get; set; } = "Demo Inspector";

    /// <summary>
    /// When the checklist was submitted.
    /// </summary>
    public DateTimeOffset SubmittedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Workflow status for demo:
    /// Draft, Submitted, PendingSupervisor, Approved, Rejected
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Submitted";

    /// <summary>
    /// Optional overall remark at submission level.
    /// </summary>
    [MaxLength(1000)]
    public string? OverallRemark { get; set; }

    /// <summary>
    /// Navigation: one submission has many answered items.
    /// </summary>
    public List<ChecklistSubmissionItem> Items { get; set; } = new();
}
