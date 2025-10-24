using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Application.Contexts.Categories;

/// <summary>
/// Репозиторий для работы с категориями (доменная модель).
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Возвращает категорию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// Экземпляр <see cref="Category"/> или <c>null</c>, если не найдено.
    /// </returns>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет новую категорию.
    /// </summary>
    /// <param name="category">Доменная сущность категории.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="category"/> равна <c>null</c>.</exception>
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаляет категорию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}