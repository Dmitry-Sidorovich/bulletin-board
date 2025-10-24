namespace BulletinBoard.Application.Abstractions;

/// <summary>
/// Сервис для получения информации о текущем авторизованном пользователе.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Возвращает ID текущего пользователя.
    /// </summary>
    /// <returns>ID пользователя или null, если пользователь не авторизован.</returns>
    Guid? GetCurrentUserId();

    /// <summary>
    /// Проверяет, является ли текущий пользователь администратором.
    /// </summary>
    bool IsAdmin();

    /// <summary>
    /// Проверяет, является ли текущий пользователь владельцем ресурса или администратором.
    /// </summary>
    bool IsOwnerOrAdmin(Guid resourceOwnerId);
}