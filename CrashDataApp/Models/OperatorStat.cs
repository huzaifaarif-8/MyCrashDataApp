namespace CrashDataApp.Models;

public class OperatorStat
{
    public int Id { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public int TotalCrashes { get; set; }
    public int TotalFatalities { get; set; }
    public int TotalAboard { get; set; }
    public int? FirstCrashYear { get; set; }
    public int? LastCrashYear { get; set; }
}
