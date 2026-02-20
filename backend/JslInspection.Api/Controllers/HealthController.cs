using Microsoft.AspNetCore.Mvc;

namespace JslInspection.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "JslInspection.Api",
            time = DateTimeOffset.UtcNow
        });
    }
}
