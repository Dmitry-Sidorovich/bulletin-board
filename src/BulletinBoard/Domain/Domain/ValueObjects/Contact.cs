namespace BulletinBoard.Domain.ValueObjects;

/// <summary>
/// Контактные данные, сохраняемые как «снимок» внутри объявления.
/// Не имеет собственного идентификатора и жизни вне объявления.
/// </summary>
public sealed class Contact
{
    /// <summary> Имя продавца.</summary>
    public string Name { get; }
    
    /// <summary> Почта продавца (обязательна для объявления).</summary>
    public string Email { get; }
    
    /// <summary> Телефон продавца (опционально).</summary>
    public string? Phone { get; }
    
    /// <summary> Пустой конструктор для EF Core.</summary>
    private Contact() {}
    
    /// <summary>
    /// Создаёт контактный «снимок» для объявления.
    /// </summary>
    /// <param name="name">Имя (обязательное, Trim).</param>
    /// <param name="email"> Почта (обязательна, Trim).</param>
    /// <param name="phone"> Телефон (необязателен, Trim).</param>
    /// <exception cref="ArgumentException">Если имя/почта пустые.</exception>
    public Contact(string name, string email, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Имя обязательно.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Почта обязательна.", nameof(email));
        }

        Name = name.Trim();
        Email = email.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
    }
}