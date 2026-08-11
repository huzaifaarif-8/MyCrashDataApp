using CrashDataApp.Models;

namespace CrashDataApp.Repositories;

public interface ICrashRepository
{
    Task<int> CountAsync();
    Task<List<Crash>> GetPageAsync(int page, int pageSize);
    Task<Crash?> GetByIdAsync(int id);
    Task<List<Crash>> GetAllAsync();
}
