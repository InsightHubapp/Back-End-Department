using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InsightHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly INewsQuery _query;

    public NewsController(INewsQuery query)
    {
        _query = query;
    }

    [HttpPost]
    public async Task<IActionResult> GetArticles([FromBody] List<FelteredTracks> userTracks, [FromQuery] int page = 1, [FromQuery] int pageSize = 15)
    {
        if (userTracks == null || userTracks.Count == 0)
        {
            return BadRequest("No tracks provided.");
        }

        var categories = userTracks
            .Select(t => t.CategoryName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        var result = await _query.GetArticlesAsync(categories, page, pageSize, HttpContext.RequestAborted);
        return Ok(result);
    }
}
