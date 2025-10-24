namespace BulletinBoard.Contracts.Auth;

/// <summary>
/// DTO для входа пользователя в систему.
/// </summary>
public sealed class LoginDto
{
    /// <summary>Email пользователя.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Пароль.</summary>
    public string Password { get; init; } = string.Empty;
}