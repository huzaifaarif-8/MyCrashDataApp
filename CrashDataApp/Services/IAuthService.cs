using CrashDataApp.DTOs;

namespace CrashDataApp.Services;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(string username, string password, string? remoteIp);
}
