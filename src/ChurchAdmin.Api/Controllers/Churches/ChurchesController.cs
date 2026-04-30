using ChurchAdmin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Api.Controllers.Churches;

[ApiController]
[Route("api/churches")]
public sealed class ChurchesController : ControllerBase
{
    private readonly ChurchAdminDbContext _dbContext;

    public ChurchesController(ChurchAdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var church = await _dbContext.Churches
            .Where(x => x.Slug == slug && x.IsActive)
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

        if (church == null)
        {
            return NotFound();
        }

        return Ok(church);
    }
}