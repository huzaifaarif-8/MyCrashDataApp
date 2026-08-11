namespace CrashDataApp.DTOs;

public class CrashSummaryDto
{
    public int TotalCrashes { get; set; }
    public int TotalAboard { get; set; }
    public int TotalFatalities { get; set; }
    public double FatalityRatePct { get; set; }
}

public class DecadeStatDto
{
    public int Decade { get; set; }
    public int Crashes { get; set; }
    public int Fatalities { get; set; }
}

public class OperatorStatDto
{
    public string Operator { get; set; } = string.Empty;
    public int Crashes { get; set; }
    public int Fatalities { get; set; }
}

public class DeadliestCrashDto
{
    public int Decade { get; set; }
    public string? Date { get; set; }
    public string? Location { get; set; }
    public string? Operator { get; set; }
    public int? Fatalities { get; set; }
}

public class AircraftTypeStatDto
{
    public string AircraftType { get; set; } = string.Empty;
    public int Crashes { get; set; }
    public int Fatalities { get; set; }
}

public class MilitaryVsCivilianDto
{
    public string Category { get; set; } = string.Empty;
    public int Crashes { get; set; }
    public int Fatalities { get; set; }
    public double AvgFatalitiesPerCrash { get; set; }
}

public class EngineFailureYearDto
{
    public int Year { get; set; }
    public int Count { get; set; }
}

public class CumulativeFatalityDto
{
    public int Year { get; set; }
    public int Fatalities { get; set; }
    public long CumulativeFatalities { get; set; }
}

public class YearOverYearDto
{
    public int Year { get; set; }
    public int Crashes { get; set; }
    public int? PreviousYearCrashes { get; set; }
    public double? PctChange { get; set; }
}

public class RegionStatDto
{
    public string Region { get; set; } = string.Empty;
    public int Crashes { get; set; }
    public int Fatalities { get; set; }
}
