using JslInspection.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JslInspection.Api.Controllers;

/// <summary>
/// Demo utility endpoints to view email queue.
/// Later a background worker will pick pending emails and send via SMTP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmailOutboxController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmailOutboxController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Shows most recent emails (all statuses).
    /// GET /api/emailoutbox/recent?take=50
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> Recent([FromQuery] int take = 50)
    {
        if (take < 1) take = 1;
        if (take > 200) take = 200;

        var rows = await _db.EmailOutbox
            .AsNoTracking()
            .OrderByDescending(e => e.Id)
            .Take(take)
            .Select(e => new
            {
                e.Id,
                e.ToEmail,
                e.CcEmail,
                e.Subject,
                e.Status,
                e.ChecklistSubmissionId,
                e.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(rows);
    }

    /// <summary>
    /// Shows only pending emails.
    /// GET /api/emailoutbox/pending
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
    {
        var rows = await _db.EmailOutbox
            .AsNoTracking()
            .Where(e => e.Status == "Pending")
            .OrderByDescending(e => e.Id)
            .Select(e => new
            {
                e.Id,
                e.ToEmail,
                e.Subject,
                e.ChecklistSubmissionId,
                e.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(rows);
    }

    /// <summary>
    /// View a specific email including HTML body.
    /// GET /api/emailoutbox/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var e = await _db.EmailOutbox
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (e == null) return NotFound("Email not found.");

        return Ok(new
        {
            e.Id,
            e.ToEmail,
            e.CcEmail,
            e.Subject,
            e.BodyHtml,
            e.Status,
            e.Error,
            e.ChecklistSubmissionId,
            e.CreatedAtUtc
        });
    }
}
