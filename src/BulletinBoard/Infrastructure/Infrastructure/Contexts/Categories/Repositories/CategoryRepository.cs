using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Infrastructure.Contexts.Categories;

/// <inheritdoc />
public sealed class CategoryRepository : ICategoryRepository
{
    /// <inheritdoc />
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}