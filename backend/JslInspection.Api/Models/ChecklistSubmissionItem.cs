using System.ComponentModel.DataAnnotations;

namespace JslInspection.Api.Models;

/// <summary>
/// One answered line inside a submission.
/// Example:
/// Oil leakage observed? -> Not OK
/// Motor temperature -> 85
/// </summary>
public class ChecklistSubmissionItem
{
    public int Id { get; set; }

    /// <summary>
    /// Parent submission (foreign key).
    /// </summary>
    public int ChecklistSubmissionId { get; set; }
    public ChecklistSubmission? ChecklistSubmission { get; set; }

    /// <summary>
    /// Which checklist template item this answer belongs to.
    /// (Links back to ChecklistItemTemplates table)
    /// </summary>
    public int ChecklistItemTemplateId { get; set; }

    /// <summary>
    /// Value stored as text for flexibility.
    /// Examples:
    /// "OK", "NotOK", "85", "Good"
    /// </summary>
    [MaxLength(200)]
    public string? ValueText { get; set; }

    /// <summary>
    /// Optional numeric value (for temperature etc.).
    /// </summary>
    public decimal? ValueNumber { get; set; }

    /// <summary>
    /// Inspector comment for this item.
    /// </summary>
    [MaxLength(500)]
    public string? Comment { get; set; }

    /// <summary>
    /// Quick flag to indicate abnormal condition.
    /// UI will set this if operator marks Not OK.
    /// </summary>
    public bool IsNotOk { get; set; }
}
