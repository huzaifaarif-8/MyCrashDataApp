namespace CrashDataApp.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }
}

public class UserOperationResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool NotFound { get; set; }
    public bool Conflict { get; set; }
}
