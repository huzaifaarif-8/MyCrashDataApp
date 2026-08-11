using CrashDataApp.DTOs;

namespace CrashDataApp.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserOperationResultDto> CreateAsync(string username, string password);
    Task<UserOperationResultDto> DeleteAsync(int id);
}
