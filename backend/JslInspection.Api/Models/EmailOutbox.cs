using System.ComponentModel.DataAnnotations;

namespace JslInspection.Api.Models;

/// <summary>
/// Stores emails that should be sent.
/// Demo: we log them in DB (safe).
/// Later: a background worker can read Pending emails and send via SMTP.
/// </summary>
public class EmailOutbox
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string ToEmail { get; set; } = default!;

    [MaxLength(200)]
    public string? CcEmail { get; set; }

    [Required]
    [MaxLength(250)]
    public string Subject { get; set; } = default!;

    [Required]
    public string BodyHtml { get; set; } = default!;

    // Optional: link email to a submission
    public int? ChecklistSubmissionId { get; set; }

    // Pending / Sent / Failed
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(2000)]
    public string? Error { get; set; }
}
