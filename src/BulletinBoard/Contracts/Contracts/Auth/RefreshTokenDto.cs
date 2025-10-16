namespace BulletinBoard.Contracts.Auth;

/// <summary>
/// DTO для обновления access token через refresh token.
/// </summary>
public sealed class RefreshTokenDto
{
    /// <summary>Refresh token.</summary>
    public string RefreshToken { get; init; } = string.Empty;
}