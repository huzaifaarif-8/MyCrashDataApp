using CrashDataApp.Models;

namespace CrashDataApp.Repositories;

public interface IUserRepository
{
    Task<List<AppUser>> GetAllAsync();
    Task<AppUser?> GetByIdAsync(int id);
    Task<AppUser?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<int> CountAsync();
    Task AddAsync(AppUser user);
    Task RemoveAsync(AppUser user);
}
