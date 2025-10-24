using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Application.Abstractions;

/// <summary>
/// Сервис для генерации и валидации JWT токенов.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Генерирует JWT access token для пользователя.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <returns>JWT токен (строка).</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Генерирует refresh token (случайный GUID).
    /// </summary>
    /// <returns>Refresh token (строка).</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Извлекает UserId из JWT токена.
    /// </summary>
    /// <param name="token">JWT токен.</param>
    /// <returns>Идентификатор пользователя или null, если токен невалидный.</returns>
    Guid? GetUserIdFromToken(string token);
}