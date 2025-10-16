using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Application.Abstractions;

/// <summary>
/// Репозиторий для работы с refresh токенами.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Находит активный refresh token по значению токена.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Refresh token или null, если не найден.</returns>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет новый refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отзывает все refresh токены пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}