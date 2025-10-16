namespace BulletinBoard.Contracts.Auth;

/// <summary>
/// DTO с результатом успешного входа.
/// </summary>
public sealed class LoginResponseDto
{
    /// <summary>JWT access token (короткоживущий, 60 минут).</summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>Refresh token (долгоживущий, 30 дней).</summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>Время жизни access token в секундах.</summary>
    public int ExpiresIn { get; init; }

    /// <summary>Информация о пользователе.</summary>
    public UserInfoDto User { get; init; } = null!;
}