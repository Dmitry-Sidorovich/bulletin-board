namespace BulletinBoard.Contracts.Auth;

/// <summary>
/// Информация о текущем пользователе.
/// </summary>
public sealed class UserInfoDto
{
    /// <summary>Идентификатор пользователя.</summary>
    public Guid Id { get; init; }

    /// <summary>Отображаемое имя.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Email.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Роль (User или Admin).</summary>
    public string Role { get; init; } = string.Empty;
}