using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Application.Contexts.Advertisements;

/// <summary>
/// Репозиторий для работы со связями объявлений и файлов.
/// </summary>
public interface IAdvertisementFileRepository
{
    /// <summary>
    /// Проверить существование объявления.
    /// </summary>
    /// <param name="advertisementId">Идентификатор объявления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>True, если объявление существует; иначе False.</returns>
    Task<bool> AdvertisementExistsAsync(Guid advertisementId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование файла.
    /// </summary>
    /// <param name="fileId">Идентификатор файла.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>True, если файл существует; иначе False.</returns>
    Task<bool> FileExistsAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, прикреплен ли файл к объявлению.
    /// </summary>
    /// <param name="advertisementId">Идентификатор объявления.</param>
    /// <param name="fileId">Идентификатор файла.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>True, если файл прикреплен к объявлению; иначе False.</returns>
    Task<bool> IsFileAttachedAsync(Guid advertisementId, Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить максимальный порядок отображения файлов для объявления.
    /// </summary>
    /// <param name="advertisementId">Идентификатор объявления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Максимальное значение Order среди файлов объявления.
    /// Возвращает -1, если у объявления нет файлов.
    /// </returns>
    Task<int> GetMaxOrderAsync(Guid advertisementId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить связь между объявлением и файлом.
    /// </summary>
    /// <param name="advertisementFile">Сущность связи объявления и файла.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task AddAsync(AdvertisementFile advertisementFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить связь между объявлением и файлом.
    /// </summary>
    /// <param name="advertisementId">Идентификатор объявления.</param>
    /// <param name="fileId">Идентификатор файла.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>True, если связь удалена; False, если связь не найдена.</returns>
    Task<bool> DeleteAsync(Guid advertisementId, Guid fileId, CancellationToken cancellationToken = default);
}