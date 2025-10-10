using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;

namespace BulletinBoard.Application.Contexts.Advertisements;

/// <summary>
/// Прикладной сервис объявлений: операции чтения и записи.
/// </summary>
public interface IAdvertisementService
{
    /// <summary>
    /// Возвращает объявление по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns><see cref="AdvertisementDto"/> или <c>null</c>.</returns>
    Task<AdvertisementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает объявления по категории с постраничным выводом.
    /// </summary>
    /// <param name="categoryId">Идентификатор категории.</param>
    /// <param name="page">Параметры пагинации.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Пагинированный результат объявлений.</returns>
    Task<PagedResult<AdvertisementDto>> GetByCategoryAsync(
        Guid categoryId,
        PageRequest page,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт новое объявление.
    /// </summary>
    /// <param name="dto">Данные для создания.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Созданное объявление.</returns>
    /// <exception cref="ArgumentException">Если входные данные некорректны.</exception>
    Task<AdvertisementDto> CreateAsync(CreateAdvertisementDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет объявление.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="dto">Новые значения полей.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Обновлённое объявление или <c>null</c>, если не найдено.</returns>
    /// <exception cref="ArgumentException">Если входные данные некорректны.</exception>
    Task<AdvertisementDto?> UpdateAsync(Guid id, UpdateAdvertisementDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Изменяет статус объявления.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="dto">Новый статус.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns><c>true</c>, если статус изменён; иначе <c>false</c>.</returns>
    Task<bool> ChangeStatusAsync(Guid id, ChangeAdvertisementStatusDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет объявление.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns><c>true</c>, если удалено; иначе <c>false</c>.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Прикрепить файл к объявлению.
    /// </summary>
    /// <param name="advertisementId">Идентификатор объявления.</param>
    /// <param name="fileId">Идентификатор файла.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// True, если файл успешно прикреплен; 
    /// False, если объявление или файл не найдены.
    /// </returns>
    Task<bool> AttachFileAsync(Guid advertisementId, Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Открепить файл от объявления.
    /// </summary>
    /// <param name="advertisementId">Идентификатор объявления.</param>
    /// <param name="fileId">Идентификатор файла.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// True, если файл успешно откреплен; 
    /// False, если привязка не найдена.
    /// </returns>
    Task<bool> DetachFileAsync(Guid advertisementId, Guid fileId, CancellationToken cancellationToken = default);
}
