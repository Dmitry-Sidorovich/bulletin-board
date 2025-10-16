using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Application.Contexts.Users;

/// <summary>
/// Репозиторий для работы с пользователями (доменная модель).
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Возвращает пользователя по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Экземпляр <see cref="User"/> или <c>null</c>, если не найден.
    /// </returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет нового пользователя.
    /// </summary>
    /// <param name="user">Доменная сущность пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="user"/> равен <c>null</c>.</exception>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет данные пользователя.
    /// </summary>
    /// <param name="user">Доменная сущность с изменениями.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="user"/> равен <c>null</c>.</exception>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Находит пользователя по email.
    /// </summary>
    /// <param name="email">Email пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Пользователь или null, если не найден.</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}