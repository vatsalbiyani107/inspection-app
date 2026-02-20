using System.ComponentModel.DataAnnotations;

namespace JslInspection.Api.Models;

/// <summary>
/// Photo attached to a specific submission item.
/// For demo, we store file path / name.
/// Later: store URL to object storage (MinIO/S3/Azure Blob).
/// </summary>
public class SubmissionPhoto
{
    public int Id { get; set; }

    /// <summary>
    /// Which answered item this photo belongs to.
    /// Example: "Oil leakage observed" item.
    /// </summary>
    public int ChecklistSubmissionItemId { get; set; }

    /// <summary>
    /// For demo: local path or filename.
    /// In real setup: URL to stored image.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string PhotoPath { get; set; } = default!;

    public DateTimeOffset CapturedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
