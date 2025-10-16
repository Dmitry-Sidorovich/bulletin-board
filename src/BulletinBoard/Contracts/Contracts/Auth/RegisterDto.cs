namespace BulletinBoard.Contracts.Auth;

/// <summary>
/// DTO для регистрации нового пользователя.
/// </summary>
public sealed class RegisterDto
{
    /// <summary>Отображаемое имя пользователя.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Email (используется для входа).</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Пароль.</summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>Телефон (необязательно).</summary>
    public string? Phone { get; init; }
}