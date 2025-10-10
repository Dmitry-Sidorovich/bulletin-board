using BulletinBoard.Domain.Base;

namespace BulletinBoard.Domain.Entities;

/// <summary>
/// Пользователь (продавец/автор объявлений).
/// </summary>
public class User : EntityBase
{
    /// <summary> Отображаемое имя пользователя.</summary>
    public string DisplayName { get; private set; } = string.Empty;
    
    /// <summary> Почта (основной контакт).</summary>
    public string Email { get; private set; } = string.Empty;
    
    /// <summary> Телефон (необязательный контакт).</summary>
    public string? Phone { get; private set; }
    
    /// <summary> Пустой конструктор для EF Core.</summary>
    private User() { }

    /// <summary>
    /// Создаёт пользователя (без учётки/авторизации).
    /// </summary>
    public User(string displayName, string email, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Имя пользователя обязательно.", nameof(displayName));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Почта обязателен.", nameof(email));
        }
        
        DisplayName = displayName.Trim();
        Email = email.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
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
    /// Бросается, если переданы пустые значения для обязательных полей.
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
    }
}