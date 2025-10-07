using BulletinBoard.Contracts.Categories;
using BulletinBoard.Contracts.Common;

namespace BulletinBoard.Application.Contexts.Categories;

/// <inheritdoc />
public sealed class CategoryService : ICategoryService
{
    /// <inheritdoc />
    public Task<IReadOnlyList<CategoryDto>> GetRootsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<PagedResult<CategoryDto>> GetChildrenAsync(Guid parentId, PageRequest page, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<CategoryDto> CreateRootAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<CategoryDto> CreateChildAsync(Guid parentId, string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}