using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Application.Contexts.Advertisements;

/// <summary>
/// Репозиторий для работы с объявлениями (доменная модель).
/// </summary>
public interface IAdvertisementRepository
{
    /// <summary>
    /// Возвращает доменное объявление по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns><see cref="Advertisement"/> или <c>null</c>, если не найдено.</returns>
    Task<Advertisement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет новое объявление (доменную сущность).
    /// </summary>
    /// <param name="ad">Доменная сущность объявления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="ad"/> равен <c>null</c>.</exception>
    Task AddAsync(Advertisement ad, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет объявление (доменную сущность).
    /// </summary>
    /// <param name="ad">Экземпляр с изменениями.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="ad"/> равен <c>null</c>.</exception>
    Task UpdateAsync(Advertisement ad, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет объявление по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}