namespace BulletinBoard.Contracts.Users;

/// <summary>
/// DTO пользователя для чтения (ответ API).
/// </summary>
public sealed class UserDto
{
    /// <summary> Идентификатор пользователя.</summary>
    public Guid Id { get; init; }

    /// <summary> Отображаемое имя.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Email (обязательно).</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Телефон (опционально).</summary>
    public string? Phone { get; init; }

    /// <summary> Время создания (UTC).</summary>
    public DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>Роль пользователя (User или Admin).</summary>
    public string Role { get; init; } = string.Empty;
}