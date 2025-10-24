using BulletinBoard.Domain.Base;

namespace BulletinBoard.Domain.Entities;

/// <summary>
/// Refresh-токен для обновления JWT access token.
/// Хранится в БД для возможности отзыва.
/// </summary>
public class RefreshToken : EntityBase
{
    /// <summary>Идентификатор пользователя-владельца токена.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Навигационное свойство к пользователю.</summary>
    public User User { get; private set; } = null!;

    /// <summary>Сам токен (уникальная строка, обычно GUID).</summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>Дата и время истечения токена (UTC).</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>Отозван ли токен (logout, смена пароля и т.д.).</summary>
    public bool IsRevoked { get; private set; }

    /// <summary>Дата и время отзыва токена (UTC).</summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// Проверяет, активен ли токен (не истёк и не отозван).
    /// </summary>
    public bool IsActive => !IsRevoked && ExpiresAt > DateTimeOffset.UtcNow;

    /// <summary>Пустой конструктор для EF Core.</summary>
    private RefreshToken() { }

    /// <summary>
    /// Создаёт новый refresh-токен.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="token">Токен.</param>
    /// <param name="expiresAt">Дата истечения (обычно +30 дней от текущего момента).</param>
    public RefreshToken(Guid userId, string token, DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId не может быть пустым.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Токен обязателен.", nameof(token));
        }

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException("Дата истечения должна быть в будущем.", nameof(expiresAt));
        }

        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        IsRevoked = false;
    }

    /// <summary>
    /// Отзывает токен (делает его неактивным).
    /// Используется при logout, смене пароля и т.д.
    /// </summary>
    public void Revoke()
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException("Токен уже отозван.");
        }

        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
    }
}