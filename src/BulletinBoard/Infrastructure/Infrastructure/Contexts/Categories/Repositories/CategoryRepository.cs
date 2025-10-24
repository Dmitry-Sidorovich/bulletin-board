using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Infrastructure.Contexts.Categories;

/// <inheritdoc />
public sealed class CategoryRepository : ICategoryRepository
{
    private readonly BulletinBoardDbContext _dbContext;

    /// <summary>
    /// Инициализирует экземпляр <see cref="CategoryRepository"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public CategoryRepository(BulletinBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <inheritdoc />
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);

        await _dbContext.Categories.AddAsync(category, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await GetByIdAsync(id, cancellationToken);
        if (category != null)
        {
            _dbContext.Categories.Remove(category);
        }
    }
}