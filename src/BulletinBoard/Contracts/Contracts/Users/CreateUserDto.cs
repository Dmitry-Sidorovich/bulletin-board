namespace BulletinBoard.Contracts.Users;

/// <summary>
/// DTO создания пользователя (запрос API).
/// </summary>
public sealed class CreateUserDto
{
    /// <summary> Отображаемое имя (обязательно).</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Email (обязателен).</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Телефон (необязателен).</summary>
    public string? Phone { get; init; }
}