using CrashDataApp.Data;
using CrashDataApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CrashDataApp.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CrashContext _context;

    public UserRepository(CrashContext context) => _context = context;

    public Task<List<AppUser>> GetAllAsync() =>
        _context.Users.OrderBy(u => u.Username).ToListAsync();

    public Task<AppUser?> GetByIdAsync(int id) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<AppUser?> GetByUsernameAsync(string username) =>
        _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public Task<bool> ExistsByUsernameAsync(string username) =>
        _context.Users.AnyAsync(u => u.Username == username);

    public Task<int> CountAsync() => _context.Users.CountAsync();

    public async Task AddAsync(AppUser user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(AppUser user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
