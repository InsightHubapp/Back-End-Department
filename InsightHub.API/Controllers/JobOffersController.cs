using InsightHub.Application.Interfaces;
using InsightHub.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InsightHub.API.Controllers;

[ApiController]
[Route("api/JobsOffers")]
public class JobOffersController : ControllerBase
{
    private readonly IJobOffersQuery _query;

    public JobOffersController(IJobOffersQuery query)
    {
        _query = query;
    }

    [HttpPost]
    public async Task<IActionResult> GetRelatedJobs([FromBody] List<FelteredTracks> userTracks, [FromQuery] int page = 1, [FromQuery] int pageSize = 15)
    {
        if (userTracks == null || userTracks.Count == 0)
        {
            return BadRequest("UserTracks is required");
        }

        var categories = userTracks
            .Select(ut => ut.CategoryName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        var result = await _query.GetRelatedJobsAsync(categories, page, pageSize, HttpContext.RequestAborted);
        return Ok(result);
    }
}
