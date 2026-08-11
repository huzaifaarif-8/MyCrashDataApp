using CrashDataApp.DTOs;
using CrashDataApp.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CrashDataApp.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository repository, IConfiguration config, ILogger<AuthService> logger)
    {
        _repository = repository;
        _config = config;
        _logger = logger;
    }

    public async Task<LoginResultDto> LoginAsync(string username, string password, string? remoteIp)
    {
        var user = await _repository.GetByUsernameAsync(username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username {Username} from {IP}", username, remoteIp);
            return new LoginResultDto { Success = false, ErrorMessage = "Invalid username or password." };
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        _logger.LogInformation("User {Username} logged in successfully", user.Username);

        return new LoginResultDto
        {
            Success = true,
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}
