using System.ComponentModel.DataAnnotations;

namespace JslInspection.Api.Models;

/// <summary>
/// One line/question inside a checklist.
/// Example: "Any oil leakage observed?"
/// </summary>
public class ChecklistItemTemplate
{
    public int Id { get; set; }

    // Foreign key: which checklist this item belongs to
    public int ChecklistTemplateId { get; set; }

    // Navigation back to parent
    public ChecklistTemplate? ChecklistTemplate { get; set; }

    // Display order in UI
    public int SeqNo { get; set; }

    // Text shown to operator
    [Required]
    [MaxLength(250)]
    public string ItemText { get; set; } = default!;

    // Input type for UI: PassFail / Number / Text / Dropdown
    [Required]
    [MaxLength(30)]
    public string InputType { get; set; } = "PassFail";

    // Required field?
    public bool IsRequired { get; set; } = true;

    // If Not OK -> photo required (rule stored in template)
    public bool RequiresPhotoIfNotOk { get; set; } = false;

    // For numeric checks (optional)
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }

    // For dropdown (demo simple approach)
    [MaxLength(300)]
    public string? DropdownOptionsCsv { get; set; }
}
