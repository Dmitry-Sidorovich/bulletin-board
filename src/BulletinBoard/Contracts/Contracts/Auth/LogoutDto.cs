namespace BulletinBoard.Contracts.Auth;

/// <summary>
/// DTO для выхода из системы (отзыв refresh token).
/// </summary>
public sealed class LogoutDto
{
    /// <summary>
    /// Refresh token для отзыва.
    /// </summary>
    public required string RefreshToken { get; init; }
}