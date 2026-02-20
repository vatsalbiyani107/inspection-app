using System.ComponentModel.DataAnnotations;

namespace JslInspection.Api.Models;

public class Equipment
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Plant { get; set; } = "JSL";

    [Required]
    [MaxLength(50)]
    public string Area { get; set; } = "CRM";

    [Required]
    [MaxLength(100)]
    public string EquipmentTag { get; set; } = default!;

    [MaxLength(200)]
    public string? EquipmentName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
