using InsightHub.Application.ViewModels;
using InsightHub.Domain.Entities;
using InsightHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InsightHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisProxyController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _daBaseUrl;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;

 

    public AnalysisProxyController(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IHttpClientFactory httpClientFactoryy,
        IOptions<DataAnalysisSettings> settings)
    {
        _httpClient = httpClientFactoryy.CreateClient();
        _daBaseUrl = settings.Value.BaseUrl;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpGet("home")]
    public async Task<IActionResult> Home()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await _dbContext.Users
            .Include(u => u.Track)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return Unauthorized();

        var requestBody = new
        {
            filters = user.Track?.Name != null
         ? new Dictionary<string, List<string>>
         {
             ["field_label"] = new List<string> { user.Track.Name }
         }
         : new Dictionary<string, List<string>>(),
           
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

        var response = await _httpClient.PostAsync($"{_daBaseUrl}/api/home", content);
        var responseData = await response.Content.ReadAsStringAsync();

        return Content(responseData, "application/json");
    }


    [Authorize]
    [HttpPost("explore")]
    public async Task<IActionResult> Explore([FromBody] ExploreRequestDto requestBody)
    {
        if (string.IsNullOrWhiteSpace(requestBody.Filters.Title?.Contains))
            requestBody.Filters.Title = null;

        if (requestBody.Filters.FieldLabel != null && !requestBody.Filters.FieldLabel.Any())
            requestBody.Filters.FieldLabel = null;

        if (requestBody.Filters.SalaryAvg != null)
        {
            var min = requestBody.Filters.SalaryAvg.Min ?? 0;
            var max = requestBody.Filters.SalaryAvg.Max ?? 0;

            if (min < 0 || max < 0)
                return BadRequest(new { Message = "Salary values must be positive." });

            if (min == 0 && max == 0)
                requestBody.Filters.SalaryAvg = null;
        }

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await _httpClient.PostAsync($"{_daBaseUrl}/api/explore", content);
        var responseData = await response.Content.ReadAsStringAsync();

        return Content(responseData, "application/json");
    }
}
