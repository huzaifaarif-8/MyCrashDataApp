using Microsoft.Data.Sqlite;
using System.Data;

namespace CrashDataApp.Data;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AnalyticsConnection")
            ?? "Data Source=analytics.db";
    }

    public IDbConnection CreateConnection() => new SqliteConnection(_connectionString);
}
