using BulletinBoard.Contracts.Users;

namespace BulletinBoard.Application.Contexts.Users;

/// <summary>
/// Read-репозиторий пользователей (чтение DTO).
/// </summary>
public interface IUserReadRepository
{
    /// <summary>
    /// Возвращает пользователя (DTO) по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns><see cref="UserDto"/> или <c>null</c>, если не найден.</returns>
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}