using CrashDataApp.Data;
using CrashDataApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CrashDataApp.Repositories;

public class CrashRepository : ICrashRepository
{
    private readonly CrashContext _context;

    public CrashRepository(CrashContext context) => _context = context;

    public Task<int> CountAsync() => _context.Crashes.CountAsync();

    public Task<List<Crash>> GetPageAsync(int page, int pageSize) =>
        _context.Crashes
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public Task<Crash?> GetByIdAsync(int id) =>
        _context.Crashes.FirstOrDefaultAsync(c => c.Id == id);

    public Task<List<Crash>> GetAllAsync() =>
        _context.Crashes.ToListAsync();
}
