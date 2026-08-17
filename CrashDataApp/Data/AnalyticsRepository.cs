using CrashDataApp.Models;
using Dapper;

namespace CrashDataApp.Data;

public class AnalyticsRepository
{
    private readonly DapperContext _context;

    public AnalyticsRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task InitializeSchemaAsync()
    {
        using var conn = _context.CreateConnection();
        await conn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS OperatorStats (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OperatorName TEXT NOT NULL UNIQUE,
                TotalCrashes INTEGER NOT NULL DEFAULT 0,
                TotalFatalities INTEGER NOT NULL DEFAULT 0,
                TotalAboard INTEGER NOT NULL DEFAULT 0,
                FirstCrashYear INTEGER,
                LastCrashYear INTEGER
            )");
    }

    public async Task UpsertOperatorStatsAsync(IEnumerable<OperatorStat> stats)
    {
        using var conn = _context.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        await conn.ExecuteAsync("DELETE FROM OperatorStats", transaction: tx);

        await conn.ExecuteAsync(@"
            INSERT INTO OperatorStats
                (OperatorName, TotalCrashes, TotalFatalities, TotalAboard, FirstCrashYear, LastCrashYear)
            VALUES
                (@OperatorName, @TotalCrashes, @TotalFatalities, @TotalAboard, @FirstCrashYear, @LastCrashYear)",
            stats, transaction: tx);

        tx.Commit();
    }

    public async Task<IEnumerable<OperatorStat>> GetTopOperatorsAsync(int limit = 20)
    {
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<OperatorStat>(
            "SELECT * FROM OperatorStats ORDER BY TotalCrashes DESC LIMIT @limit",
            new { limit });
    }

    public async Task<IEnumerable<OperatorStat>> GetAllOperatorsAsync()
    {
        using var conn = _context.CreateConnection();
        return await conn.QueryAsync<OperatorStat>(
            "SELECT * FROM OperatorStats ORDER BY TotalCrashes DESC");
    }

    public async Task<OperatorStat?> GetOperatorByNameAsync(string name)
    {
        using var conn = _context.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<OperatorStat>(
            "SELECT * FROM OperatorStats WHERE OperatorName = @name",
            new { name });
    }
}
