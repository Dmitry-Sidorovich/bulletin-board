using BulletinBoard.Contracts.Categories;
using BulletinBoard.Contracts.Common;

namespace BulletinBoard.Application.Contexts.Categories;

/// <summary>
/// Read-репозиторий категорий (чтение DTO).
/// </summary>
public interface ICategoryReadRepository
{
    /// <summary>
    /// Возвращает корневые категории (без родителя).
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список категорий верхнего уровня.</returns>
    Task<IReadOnlyList<CategoryDto>> GetRootsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает дочерние категории указанного родителя с постраничным выводом.
    /// </summary>
    /// <param name="parentId">Идентификатор родительской категории.</param>
    /// <param name="page">Параметры пагинации.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Пагинированный список дочерних категорий.</returns>
    Task<PagedResult<CategoryDto>> GetChildrenAsync(
        Guid parentId,
        PageRequest page,
        CancellationToken cancellationToken = default);
}