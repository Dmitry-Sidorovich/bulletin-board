using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;

namespace BulletinBoard.Application.Contexts.Advertisements;

/// <summary>
/// Read-репозиторий объявлений (чтение DTO, постраничные выборки).
/// </summary>
public interface IAdvertisementReadRepository
{
    /// <summary>
    /// Возвращает объявление (DTO) по идентификатору.
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
    /// <exception cref="ArgumentOutOfRangeException">
    /// Если <see cref="PageRequest.Page"/> &lt; 1 или <see cref="PageRequest.PageSize"/> &lt;= 0.
    /// </exception>
    Task<PagedResult<AdvertisementDto>> GetByCategoryAsync(
        Guid categoryId,
        PageRequest page,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Возвращает объявления пользователя с пагинацией.
    /// </summary>
    Task<PagedResult<AdvertisementDto>> GetByAuthorAsync(
        Guid authorId, 
        PageRequest page, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Выполняет поиск объявлений с фильтрацией и сортировкой.
    /// </summary>
    /// <param name="filter">Параметры фильтрации и сортировки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Пагинированный результат объявлений.</returns>
    Task<PagedResult<AdvertisementDto>> SearchAsync(
        AdvertisementFilterDto filter, 
        CancellationToken cancellationToken = default);
}