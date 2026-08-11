using CrashDataApp.Models;
using CrashDataApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrashDataApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _userService.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LoginRequest request)
    {
        var result = await _userService.CreateAsync(request.Username, request.Password);

        if (!result.Success)
        {
            return result.Conflict
                ? Conflict(new { message = result.ErrorMessage })
                : BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new { message = "User created successfully." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);

        if (!result.Success)
        {
            if (result.NotFound) return NotFound();
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }
}
