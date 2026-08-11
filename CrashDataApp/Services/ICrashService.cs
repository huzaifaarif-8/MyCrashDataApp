using CrashDataApp.DTOs;
using CrashDataApp.Models;

namespace CrashDataApp.Services;

public interface ICrashService
{
    Task<PagedResult<Crash>> GetPagedCrashesAsync(int page, int pageSize);
    Task<Crash?> GetByIdAsync(int id);
    Task<CrashSummaryDto> GetSummaryAsync();
    Task<List<DecadeStatDto>> GetByDecadeAsync();
    Task<List<OperatorStatDto>> GetTopOperatorsAsync(int top);
    Task<List<DeadliestCrashDto>> GetDeadliestPerDecadeAsync();
    Task<List<AircraftTypeStatDto>> GetTopAircraftTypesAsync(int top);
    Task<List<MilitaryVsCivilianDto>> GetMilitaryVsCivilianAsync();
    Task<List<EngineFailureYearDto>> GetEngineFailureYearsAsync(int top);
    Task<List<CumulativeFatalityDto>> GetCumulativeFatalitiesAsync(int lastYears);
    Task<List<YearOverYearDto>> GetYearOverYearAsync(int lastYears);
    Task<List<RegionStatDto>> GetTopRegionsAsync(int top);
}
