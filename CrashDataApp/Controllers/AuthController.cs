using CrashDataApp.Models;
using CrashDataApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrashDataApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(
            request.Username, request.Password, HttpContext.Connection.RemoteIpAddress?.ToString());

        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        return Ok(new { token = result.Token });
    }
}
