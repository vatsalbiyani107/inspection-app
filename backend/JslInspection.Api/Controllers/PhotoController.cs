using JslInspection.Api.Data;
using JslInspection.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JslInspection.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhotoController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public PhotoController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    /// <summary>
    /// Upload request for multipart/form-data.
    /// Field names must match these property names:
    /// - ChecklistSubmissionItemId (int)
    /// - File (binary)
    /// </summary>
    public sealed class PhotoUploadRequest
    {
        public int ChecklistSubmissionItemId { get; set; }

        // Keep property name "File" so client form-data key = "File"
        public IFormFile File { get; set; } = default!;
    }

    /// <summary>
    /// Upload a photo for a specific submission item (answer).
    /// multipart/form-data:
    /// - ChecklistSubmissionItemId (int)
    /// - File (binary)
    /// </summary>
    /// POST /api/photo/upload
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(20_000_000)] // 20MB
    public async Task<IActionResult> Upload([FromForm] PhotoUploadRequest req)
    {
        // 1) Validate inputs
        if (req.ChecklistSubmissionItemId <= 0)
            return BadRequest("ChecklistSubmissionItemId is required.");

        if (req.File == null || req.File.Length == 0)
            return BadRequest("File is required.");

        // 2) Ensure the submission-item (answer row) exists
        var exists = await _db.ChecklistSubmissionItems
            .AsNoTracking()
            .AnyAsync(x => x.Id == req.ChecklistSubmissionItemId);

        if (!exists)
            return NotFound("Submission item not found.");

        // 3) Ensure uploads folder exists (under API project root)
        var uploadDir = Path.Combine(_env.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadDir);

        // 4) Create unique filename
        var ext = Path.GetExtension(req.File.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

        var safeName =
            $"subitem_{req.ChecklistSubmissionItemId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadDir, safeName);

        // 5) Save file to disk
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await req.File.CopyToAsync(stream);
        }

        // 6) Save DB record
        var photo = new SubmissionPhoto
        {
            ChecklistSubmissionItemId = req.ChecklistSubmissionItemId,
            PhotoPath = $"uploads/{safeName}", // will be served from /uploads/<filename>
            CapturedAtUtc = DateTimeOffset.UtcNow
        };

        _db.SubmissionPhotos.Add(photo);
        await _db.SaveChangesAsync();

        // 7) Return path to client
        return Ok(new
        {
            photoId = photo.Id,
            photoPath = photo.PhotoPath,
            url = $"/{photo.PhotoPath}" // e.g. /uploads/xyz.jpg
        });
    }
}
