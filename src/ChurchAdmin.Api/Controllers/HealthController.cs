using Microsoft.AspNetCore.Mvc;

namespace ChurchAdmin.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            application = "ChurchAdmin.Api",
            utc = DateTimeOffset.UtcNow
        });
    }
}
