using BulletinBoard.Contracts.Auth;

namespace BulletinBoard.Application.Contexts.Auth;

/// <summary>
/// Сервис аутентификации и авторизации.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Регистрирует нового пользователя.
    /// </summary>
    /// <param name="dto">Данные для регистрации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Информация о созданном пользователе с токенами.</returns>
    Task<LoginResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет вход пользователя.
    /// </summary>
    /// <param name="dto">Данные для входа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Токены доступа.</returns>
    Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет access token с помощью refresh token.
    /// </summary>
    /// <param name="dto">Refresh token.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Новые токены.</returns>
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет выход пользователя (отзывает refresh token).
    /// </summary>
    /// <param name="refreshToken">Refresh token для отзыва.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}