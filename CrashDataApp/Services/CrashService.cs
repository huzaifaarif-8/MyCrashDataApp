using CrashDataApp.DTOs;
using CrashDataApp.Models;
using CrashDataApp.Repositories;

namespace CrashDataApp.Services;

public class CrashService : ICrashService
{
    private readonly ICrashRepository _repository;
    private readonly ILogger<CrashService> _logger;

    public CrashService(ICrashRepository repository, ILogger<CrashService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<Crash>> GetPagedCrashesAsync(int page, int pageSize)
    {
        _logger.LogDebug("Fetching crashes page {Page} with page size {PageSize}", page, pageSize);

        var total = await _repository.CountAsync();
        var items = await _repository.GetPageAsync(page, pageSize);

        return new PagedResult<Crash>
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items
        };
    }

    public Task<Crash?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public async Task<CrashSummaryDto> GetSummaryAsync()
    {
        var crashes = await _repository.GetAllAsync();

        var totalAboard = crashes.Sum(c => c.Aboard ?? 0);
        var totalFatalities = crashes.Sum(c => c.Fatalities ?? 0);
        var fatalityRate = totalAboard == 0 ? 0 : Math.Round(100.0 * totalFatalities / totalAboard, 2);

        _logger.LogInformation(
            "Summary computed — {Total} crashes, {Fatalities} fatalities, {Rate}% fatality rate",
            crashes.Count, totalFatalities, fatalityRate);

        return new CrashSummaryDto
        {
            TotalCrashes = crashes.Count,
            TotalAboard = totalAboard,
            TotalFatalities = totalFatalities,
            FatalityRatePct = fatalityRate
        };
    }

    public async Task<List<DecadeStatDto>> GetByDecadeAsync()
    {
        var crashes = await _repository.GetAllAsync();

        return crashes
            .Where(c => c.Year != null)
            .GroupBy(c => (c.Year!.Value / 10) * 10)
            .Select(g => new DecadeStatDto
            {
                Decade = g.Key,
                Crashes = g.Count(),
                Fatalities = g.Sum(c => c.Fatalities ?? 0)
            })
            .OrderBy(x => x.Decade)
            .ToList();
    }

    public async Task<List<OperatorStatDto>> GetTopOperatorsAsync(int top)
    {
        _logger.LogDebug("Fetching top {Top} operators by fatalities", top);
        var crashes = await _repository.GetAllAsync();

        return crashes
            .Where(c => c.Operator != null)
            .GroupBy(c => c.Operator!)
            .Select(g => new OperatorStatDto
            {
                Operator = g.Key,
                Crashes = g.Count(),
                Fatalities = g.Sum(c => c.Fatalities ?? 0)
            })
            .OrderByDescending(x => x.Fatalities)
            .Take(top)
            .ToList();
    }

    public async Task<List<DeadliestCrashDto>> GetDeadliestPerDecadeAsync()
    {
        var crashes = await _repository.GetAllAsync();

        return crashes
            .Where(c => c.Year != null && c.Fatalities != null)
            .GroupBy(c => (c.Year!.Value / 10) * 10)
            .Select(g => g.OrderByDescending(c => c.Fatalities).First())
            .OrderBy(c => c.Year)
            .Select(c => new DeadliestCrashDto
            {
                Decade = (c.Year!.Value / 10) * 10,
                Date = c.Date,
                Location = c.Location,
                Operator = c.Operator,
                Fatalities = c.Fatalities
            })
            .ToList();
    }

    public async Task<List<AircraftTypeStatDto>> GetTopAircraftTypesAsync(int top)
    {
        var crashes = await _repository.GetAllAsync();

        return crashes
            .Where(c => c.AircraftType != null)
            .GroupBy(c => c.AircraftType!)
            .Select(g => new AircraftTypeStatDto
            {
                AircraftType = g.Key,
                Crashes = g.Count(),
                Fatalities = g.Sum(c => c.Fatalities ?? 0)
            })
            .OrderByDescending(x => x.Crashes)
            .Take(top)
            .ToList();
    }

    public async Task<List<MilitaryVsCivilianDto>> GetMilitaryVsCivilianAsync()
    {
        var crashes = await _repository.GetAllAsync();

        return crashes
            .GroupBy(c => c.Operator != null && c.Operator.StartsWith("Military") ? "Military" : "Civilian/Other")
            .Select(g => new MilitaryVsCivilianDto
            {
                Category = g.Key,
                Crashes = g.Count(),
                Fatalities = g.Sum(c => c.Fatalities ?? 0),
                AvgFatalitiesPerCrash = Math.Round(g.Average(c => c.Fatalities ?? 0), 2)
            })
            .ToList();
    }

    public async Task<List<EngineFailureYearDto>> GetEngineFailureYearsAsync(int top)
    {
        var crashes = await _repository.GetAllAsync();

        return crashes
            .Where(c => c.Year != null && c.Summary != null && c.Summary.Contains("engine failure"))
            .GroupBy(c => c.Year!.Value)
            .Select(g => new EngineFailureYearDto { Year = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToList();
    }

    public async Task<List<CumulativeFatalityDto>> GetCumulativeFatalitiesAsync(int lastYears)
    {
        var crashes = await _repository.GetAllAsync();

        var yearly = crashes
            .Where(c => c.Year != null)
            .GroupBy(c => c.Year!.Value)
            .Select(g => new { Year = g.Key, Fatalities = g.Sum(c => c.Fatalities ?? 0) })
            .OrderBy(x => x.Year)
            .ToList();

        long running = 0;
        return yearly
            .Select(y =>
            {
                running += y.Fatalities;
                return new CumulativeFatalityDto
                {
                    Year = y.Year,
                    Fatalities = y.Fatalities,
                    CumulativeFatalities = running
                };
            })
            .OrderByDescending(x => x.Year)
            .Take(lastYears)
            .ToList();
    }

    public async Task<List<YearOverYearDto>> GetYearOverYearAsync(int lastYears)
    {
        var crashes = await _repository.GetAllAsync();

        var yearly = crashes
            .Where(c => c.Year != null)
            .GroupBy(c => c.Year!.Value)
            .Select(g => new { Year = g.Key, Crashes = g.Count() })
            .OrderBy(x => x.Year)
            .ToList();

        return yearly
            .Select((y, i) => new YearOverYearDto
            {
                Year = y.Year,
                Crashes = y.Crashes,
                PreviousYearCrashes = i > 0 ? yearly[i - 1].Crashes : null,
                PctChange = i > 0 && yearly[i - 1].Crashes != 0
                    ? Math.Round(100.0 * (y.Crashes - yearly[i - 1].Crashes) / yearly[i - 1].Crashes, 1)
                    : null
            })
            .OrderByDescending(x => x.Year)
            .Take(lastYears)
            .ToList();
    }

    public async Task<List<RegionStatDto>> GetTopRegionsAsync(int top)
    {
        var crashes = await _repository.GetAllAsync();

        return crashes
            .Where(c => c.Location != null && c.Location.Contains(','))
            .GroupBy(c => c.Location!.Substring(c.Location.LastIndexOf(',') + 1).Trim())
            .Select(g => new RegionStatDto
            {
                Region = g.Key,
                Crashes = g.Count(),
                Fatalities = g.Sum(c => c.Fatalities ?? 0)
            })
            .OrderByDescending(x => x.Fatalities)
            .Take(top)
            .ToList();
    }
}
