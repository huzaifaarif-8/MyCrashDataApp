using CrashDataApp.DTOs;
using CrashDataApp.Models;
using CrashDataApp.Repositories;

namespace CrashDataApp.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository) => _repository = repository;

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(u => new UserDto { Id = u.Id, Username = u.Username }).ToList();
    }

    public async Task<UserOperationResultDto> CreateAsync(string username, string password)
    {
        if (await _repository.ExistsByUsernameAsync(username))
        {
            return new UserOperationResultDto
            {
                Success = false,
                Conflict = true,
                ErrorMessage = "Username already exists."
            };
        }

        await _repository.AddAsync(new AppUser
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        });

        return new UserOperationResultDto { Success = true };
    }

    public async Task<UserOperationResultDto> DeleteAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
        {
            return new UserOperationResultDto { Success = false, NotFound = true };
        }

        if (await _repository.CountAsync() == 1)
        {
            return new UserOperationResultDto
            {
                Success = false,
                ErrorMessage = "Cannot delete the last user."
            };
        }

        await _repository.RemoveAsync(user);
        return new UserOperationResultDto { Success = true };
    }
}
