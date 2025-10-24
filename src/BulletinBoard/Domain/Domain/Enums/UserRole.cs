namespace BulletinBoard.Domain.Enums;

/// <summary>
/// Роли пользователей в системе.
/// </summary>
public enum UserRole
{
    /// <summary>Обычный пользователь (может создавать объявления).</summary>
    User = 0,

    /// <summary>Администратор (полный доступ).</summary>
    Admin = 1
}