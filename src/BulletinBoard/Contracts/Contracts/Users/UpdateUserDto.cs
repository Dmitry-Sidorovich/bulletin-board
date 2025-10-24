namespace BulletinBoard.Contracts.Users;

/// <summary>
/// DTO частичного обновления пользователя (запрос API).
/// Любое поле можно опустить (оставить <c>null</c>), тогда оно не меняется.
/// </summary>
public sealed class UpdateUserDto
{
    /// <summary> Новое имя (если указано).</summary>
    public string? DisplayName { get; init; }

    /// <summary> Новый email (если указан).</summary>
    public string? Email { get; init; }

    /// <summary> Новый телефон (если указан; пустая строка трактуется как очистка на уровне сервиса/домена).</summary>
    public string? Phone { get; init; }
}