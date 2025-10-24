using BulletinBoard.Contracts.Categories;
using BulletinBoard.Contracts.Common;

namespace BulletinBoard.Application.Contexts.Categories;

/// <summary>
/// Прикладной сервис категорий: чтение и запись.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Возвращает корневые категории. (<c>ParentId = null</c>)
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Неизменяемый список категорий верхнего уровня.</returns>
    Task<IReadOnlyList<CategoryDto>> GetRootsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает дочерние категории указанного родителя с постраничным выводом.
    /// </summary>
    /// <param name="parentId">Идентификатор родительской категории.</param>
    /// <param name="page">Параметры пагинации (номер страницы и размер).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Пагинированный список дочерних категорий.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Если <paramref name="page"/> содержит некорректные значения (Page &lt; 1 или PageSize &lt;= 0).
    /// </exception>
    Task<PagedResult<CategoryDto>> GetChildrenAsync(
        Guid parentId,
        PageRequest page,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт корневую категорию.
    /// </summary>
    /// <param name="name">Название категории (обязательно, непустая строка).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Созданная категория.</returns>
    /// <exception cref="ArgumentException">
    /// Если <paramref name="name"/> пустая или состоит только из пробелов.
    /// </exception>
    Task<CategoryDto> CreateRootAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт дочернюю категорию.
    /// </summary>
    /// <param name="parentId">Идентификатор родительской категории (не <see cref="Guid.Empty"/>).</param>
    /// <param name="name">Название категории (обязательно, непустая строка).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Созданная категория.</returns>
    /// <exception cref="ArgumentException">
    /// Если <paramref name="name"/> пустая или <paramref name="parentId"/> равен <see cref="Guid.Empty"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Если родительская категория с идентификатором <paramref name="parentId"/> не найдена.
    /// </exception>
    Task<CategoryDto> CreateChildAsync(Guid parentId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет категорию.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// <c>true</c>, если категория удалена; иначе <c>false</c> (например, если не найдена).
    /// </returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Возвращает категорию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>
    /// DTO категории или <c>null</c>, если не найдена.
    /// </returns>
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
