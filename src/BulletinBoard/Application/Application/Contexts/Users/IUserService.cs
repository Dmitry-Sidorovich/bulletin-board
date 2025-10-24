using BulletinBoard.Contracts.Common;
using BulletinBoard.Contracts.Users;

namespace BulletinBoard.Application.Contexts.Users;

/// <summary>
/// Прикладной сервис пользователей: операции чтения и изменения.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Возвращает пользователя по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// <see cref="UserDto"/> или <c>null</c>, если пользователь не найден.
    /// </returns>
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает пользователей с постраничным выводом.
    /// </summary>
    /// <param name="page">Параметры пагинации (номер страницы и размер).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Пагинированный результат пользователей.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Если <paramref name="page"/> содержит некорректные значения (Page &lt; 1 или PageSize &lt;= 0).
    /// </exception>
    Task<PagedResult<UserDto>> GetPageAsync(
        PageRequest page,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновляет данные пользователя.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="dto">Изменяемые поля.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Обновлённый пользователь или <c>null</c>, если пользователь не найден.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Если входные данные некорректны (например, обязательные поля пусты при передаче).
    /// </exception>
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);
}
