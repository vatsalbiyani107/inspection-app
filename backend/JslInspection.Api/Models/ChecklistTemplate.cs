using System.ComponentModel.DataAnnotations;

namespace JslInspection.Api.Models;

/// <summary>
/// Master record for a checklist template.
/// Dropdown page will show these checklist names.
/// </summary>
public class ChecklistTemplate
{
    public int Id { get; set; }

    // Checklist display name shown in dropdown
    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = default!;

    // Optional short description
    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // When this template was created (useful for audits)
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    // Navigation: One template contains many checklist items
    public List<ChecklistItemTemplate> Items { get; set; } = new();
}
