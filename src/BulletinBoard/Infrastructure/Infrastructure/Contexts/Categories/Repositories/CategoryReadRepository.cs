using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Contracts.Categories;
using BulletinBoard.Contracts.Common;

namespace BulletinBoard.Infrastructure.Contexts.Categories;

/// <inheritdoc />
public sealed class CategoryReadRepository: ICategoryReadRepository
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
}