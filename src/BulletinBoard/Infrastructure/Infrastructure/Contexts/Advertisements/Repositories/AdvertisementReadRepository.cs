using AutoMapper;
using AutoMapper.QueryableExtensions;
using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Infrastructure.Contexts.Advertisements;

/// <inheritdoc />
public sealed class AdvertisementReadRepository : IAdvertisementReadRepository
{
    private readonly BulletinBoardDbContext _dbContext;
    private readonly IMapper _mapper;

    /// <summary>
    /// Инициализирует экземпляр <see cref="AdvertisementReadRepository"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="mapper">Маппер для преобразования сущностей в DTO.</param>
    public AdvertisementReadRepository(BulletinBoardDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    /// <inheritdoc />
    public async Task<AdvertisementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Advertisements
            .AsNoTracking()
            .Where(a => a.Id == id)
            .ProjectTo<AdvertisementDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedResult<AdvertisementDto>> GetByCategoryAsync(
        Guid categoryId,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        if (page.Page < 1 || page.PageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Invalid pagination parameters.");
        }

        var query = _dbContext.Advertisements
            .AsNoTracking()
            .Where(a => a.CategoryId == categoryId);
        
        var total = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(a => a.CreatedAt) // Новые первыми
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .ProjectTo<AdvertisementDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<AdvertisementDto>
        {
            Items = items,
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = total
        };
    }
}