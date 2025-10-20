using AutoMapper;
using AutoMapper.QueryableExtensions;
using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Contracts.Categories;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Infrastructure.Contexts.Categories.Repositories;

/// <summary>EF-репозиторий чтения категорий.</summary>
public sealed class CategoryReadRepository : ICategoryReadRepository
{
    private readonly BulletinBoardDbContext _dbContext;
    private readonly IMapper _mapper;

    /// <inheritdoc />
    public CategoryReadRepository(BulletinBoardDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryDto>> GetRootsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.ParentId == null)
            .OrderBy(c => c.Name)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedResult<CategoryDto>> GetChildrenAsync(
        Guid parentId,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        if (page.Page < 1 || page.PageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Invalid pagination parameters.");
        }

        var query = _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.ParentId == parentId);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<CategoryDto>
        {
            Items = items,
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = total
        };
    }
    
    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking() // <-- Не забываем для операций чтения
            .Where(c => c.Id == id)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider) // <-- Используем AutoMapper
            .FirstOrDefaultAsync(cancellationToken);
    }
}