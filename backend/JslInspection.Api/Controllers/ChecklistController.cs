using JslInspection.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JslInspection.Api.Controllers;

/// <summary>
/// DEMO endpoints:
/// 1) GET /api/checklist -> dropdown list of checklist names
/// 2) GET /api/checklist/{id} -> checklist items to render the form
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChecklistController : ControllerBase
{
    private readonly AppDbContext _db;

    public ChecklistController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns active checklist templates for dropdown.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _db.ChecklistTemplates
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Description
            })
            .ToListAsync();

        return Ok(templates);
    }

    /// <summary>
    /// Returns one checklist template + its ordered items.
    /// UI uses this to generate checklist screen dynamically.
    /// </summary>
    [HttpGet("{templateId:int}")]
    public async Task<IActionResult> GetTemplateDetails(int templateId)
    {
        var template = await _db.ChecklistTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == templateId && t.IsActive);

        if (template == null)
            return NotFound("Checklist template not found (or inactive).");

        var items = await _db.ChecklistItemTemplates
            .AsNoTracking()
            .Where(i => i.ChecklistTemplateId == templateId)
            .OrderBy(i => i.SeqNo)
            .Select(i => new
            {
                i.Id,
                i.SeqNo,
                i.ItemText,
                i.InputType,
                i.IsRequired,
                i.RequiresPhotoIfNotOk,
                i.MinValue,
                i.MaxValue,
                i.DropdownOptionsCsv
            })
            .ToListAsync();

        return Ok(new
        {
            template.Id,
            template.Name,
            template.Description,
            Items = items
        });
    }
}
