using ChurchAdmin.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Api.Controllers.Churches;

[ApiController]
[Route("api/churches")]
public sealed class ChurchesController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;

    public ChurchesController(IChurchAdminDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns public branding information for a church portal.
    /// Used by the frontend login page before authentication.
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest("Church slug is required.");
        }

        var church = await _db.Churches
            .Where(x => x.Slug == slug.Trim().ToLowerInvariant() && x.IsActive && !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Slug,
                x.LogoUrl,
                x.PrimaryColor,
                x.SecondaryColor,
                x.WelcomeText
            })
            .FirstOrDefaultAsync();

        if (church is null)
        {
            return NotFound("Church portal not found.");
        }

        return Ok(church);
    }
}
