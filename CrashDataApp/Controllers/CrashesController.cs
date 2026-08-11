using CrashDataApp.Models;
using CrashDataApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrashDataApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CrashesController : ControllerBase
{
    private readonly ICrashService _crashService;

    public CrashesController(ICrashService crashService) => _crashService = crashService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery query)
    {
        var result = await _crashService.GetPagedCrashesAsync(query.Page, query.PageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var crash = await _crashService.GetByIdAsync(id);
        return crash is null ? NotFound() : Ok(crash);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary() => Ok(await _crashService.GetSummaryAsync());

    [HttpGet("by-decade")]
    public async Task<IActionResult> GetByDecade() => Ok(await _crashService.GetByDecadeAsync());

    [HttpGet("top-operators")]
    public async Task<IActionResult> GetTopOperators([FromQuery] int top = 10) =>
        Ok(await _crashService.GetTopOperatorsAsync(top));

    [HttpGet("deadliest-per-decade")]
    public async Task<IActionResult> GetDeadliestPerDecade() =>
        Ok(await _crashService.GetDeadliestPerDecadeAsync());

    [HttpGet("top-aircraft-types")]
    public async Task<IActionResult> GetTopAircraftTypes([FromQuery] int top = 8) =>
        Ok(await _crashService.GetTopAircraftTypesAsync(top));

    [HttpGet("military-vs-civilian")]
    public async Task<IActionResult> GetMilitaryVsCivilian() =>
        Ok(await _crashService.GetMilitaryVsCivilianAsync());

    [HttpGet("engine-failure")]
    public async Task<IActionResult> GetEngineFailureYears([FromQuery] int top = 10) =>
        Ok(await _crashService.GetEngineFailureYearsAsync(top));

    [HttpGet("cumulative-fatalities")]
    public async Task<IActionResult> GetCumulativeFatalities([FromQuery] int lastYears = 10) =>
        Ok(await _crashService.GetCumulativeFatalitiesAsync(lastYears));

    [HttpGet("year-over-year")]
    public async Task<IActionResult> GetYearOverYear([FromQuery] int lastYears = 10) =>
        Ok(await _crashService.GetYearOverYearAsync(lastYears));

    [HttpGet("top-regions")]
    public async Task<IActionResult> GetTopRegions([FromQuery] int top = 10) =>
        Ok(await _crashService.GetTopRegionsAsync(top));
}
