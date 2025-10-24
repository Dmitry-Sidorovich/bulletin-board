using BulletinBoard.Domain.Base;
using BulletinBoard.Domain.Enums;

namespace BulletinBoard.Domain.Entities;

/// <summary>
/// Пользователь (автор объявления).
/// </summary>
public class User : EntityBase
{
    /// <summary> Отображаемое имя пользователя.</summary>
    public string DisplayName { get; private set; } = string.Empty;
    
    /// <summary> Почта (основной контакт, используется для входа).</summary>
    public string Email { get; private set; } = string.Empty;
    
    /// <summary> Телефон (необязательный контакт).</summary>
    public string? Phone { get; private set; }
    
    /// <summary>Хеш пароля (BCrypt).</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Роль пользователя (User или Admin).</summary>
    public UserRole Role { get; private set; }

    /// <summary>Коллекция refresh-токенов пользователя.</summary>
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    private readonly List<RefreshToken> _refreshTokens = new();
    
    /// <summary> Пустой конструктор для EF Core.</summary>
    private User() { }

    /// <summary>
    /// Создаёт нового пользователя с учётными данными.
    /// </summary>
    /// <param name="displayName">Отображаемое имя.</param>
    /// <param name="email">Email (для входа).</param>
    /// <param name="passwordHash">Хеш пароля (BCrypt).</param>
    /// <param name="phone">Телефон (необязательно).</param>
    /// <param name="role">Роль (по умолчанию User).</param>
    public User(string displayName, string email, string passwordHash, string? phone = null, UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Имя пользователя обязательно.", nameof(displayName));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Почта обязательна.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Хеш пароля обязателен.", nameof(passwordHash));
        }
        
        DisplayName = displayName.Trim();
        Email = email.Trim();
        PasswordHash = passwordHash;
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Role = role;
    }

    /// <summary>
    /// Частичное обновление профиля пользователя.
    /// Любой параметр можно не передавать (оставить <c>null</c>) — тогда текущее значение не меняется.
    /// </summary>
    /// <param name="displayName">
    /// Новое имя (если указано — обязательно непустое, будет триммировано).
    /// </param>
    /// <param name="email">
    /// Новый email (если указан — обязателен и будет триммирован).
    /// </param>
    /// <param name="phone">
    /// Новый телефон (опционально). Пустая/пробельная строка очистит телефон до <c>null</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Если переданы пустые значения для обязательных полей.
    /// </exception>
    public void UpdateUser(string? displayName = null, string? email = null, string? phone = null)
    {
        if (displayName is not null)
        {
            DisplayName = string.IsNullOrWhiteSpace(displayName) 
                ? throw new ArgumentException("Имя пользователя обязательно, если указано.", nameof(displayName))
                : displayName.Trim();
        }
        
        if (email is not null)
        {
            Email = string.IsNullOrWhiteSpace(email) 
                ? throw new ArgumentException("Почта обязательна, если указана.", nameof(email))
                : email.Trim();
        }

        if (phone is not null)
        {
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        }
        
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Обновляет пароль пользователя.
    /// </summary>
    /// <param name="newPasswordHash">Новый хеш пароля (BCrypt).</param>
    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new ArgumentException("Хеш пароля обязателен.", nameof(newPasswordHash));
        }

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Повышает пользователя до администратора.
    /// </summary>
    public void PromoteToAdmin()
    {
        Role = UserRole.Admin;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Понижает администратора до обычного пользователя.
    /// </summary>
    public void DemoteToUser()
    {
        Role = UserRole.User;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}