using AutoMapper;
using AutoMapper.QueryableExtensions;
using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Contracts.Users;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Infrastructure.Contexts.Users.Repositories;

/// <inheritdoc />
public sealed class UserReadRepository: IUserReadRepository
{
    private readonly BulletinBoardDbContext _dbContext;
    private readonly IMapper _mapper;

    /// <summary>
    /// Инициализирует экземпляр <see cref="UserReadRepository"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    /// <param name="mapper">Маппер для преобразования сущностей в DTO.</param>
    public UserReadRepository(BulletinBoardDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    /// <inheritdoc />
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<PagedResult<UserDto>> GetPageAsync(
        PageRequest page, 
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _dbContext.Users.AsNoTracking();
        
        var total = await baseQuery.CountAsync(cancellationToken);
        
        var items = await baseQuery
            .OrderBy(u => u.DisplayName)
            .ThenBy(u => u.CreatedAt)
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    
        return new PagedResult<UserDto>
        {
            Items = items,
            TotalCount = total,
            Page = page.Page,
            PageSize = page.PageSize,
        };
    }
}