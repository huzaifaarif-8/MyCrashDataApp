using CrashDataApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrashDataApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsRepository _analytics;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(AnalyticsRepository analytics, ILogger<AnalyticsController> logger)
    {
        _analytics = analytics;
        _logger = logger;
    }

    [HttpGet("operators")]
    public async Task<IActionResult> GetAllOperators()
    {
        _logger.LogInformation("Fetching all operator stats from Dapper/analytics.db");
        var stats = await _analytics.GetAllOperatorsAsync();
        return Ok(stats);
    }

    [HttpGet("operators/top")]
    public async Task<IActionResult> GetTopOperators([FromQuery] int limit = 20)
    {
        _logger.LogInformation("Fetching top {Limit} operators from Dapper/analytics.db", limit);
        var stats = await _analytics.GetTopOperatorsAsync(limit);
        return Ok(stats);
    }

    [HttpGet("operators/{name}")]
    public async Task<IActionResult> GetOperatorByName(string name)
    {
        var stat = await _analytics.GetOperatorByNameAsync(name);
        if (stat is null)
            return NotFound(new { message = $"Operator '{name}' not found" });
        return Ok(stat);
    }
}
