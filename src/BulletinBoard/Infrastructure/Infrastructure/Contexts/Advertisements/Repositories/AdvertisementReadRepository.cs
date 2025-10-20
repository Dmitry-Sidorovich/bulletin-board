using AutoMapper;
using AutoMapper.QueryableExtensions;
using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Contracts.Files;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Infrastructure.Contexts.Advertisements.Repositories;

/// <inheritdoc />
public sealed class AdvertisementReadRepository : IAdvertisementReadRepository
{
    private readonly BulletinBoardDbContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Инициализирует репозиторий для чтения объявлений.
    /// </summary>
    public AdvertisementReadRepository(BulletinBoardDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<AdvertisementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await _context.Advertisements
            .AsNoTracking()
            .Where(a => a.Id == id)
            .ProjectTo<AdvertisementDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (dto != null)
        {
            dto.Files = await GetFilesAsync(id, cancellationToken);
        }

        return dto;
    }

    /// <inheritdoc />
    public async Task<PagedResult<AdvertisementDto>> GetByCategoryAsync(
        Guid categoryId,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Advertisements
            .AsNoTracking()
            .Where(a => a.CategoryId == categoryId);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .ProjectTo<AdvertisementDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Загружаем файлы для каждого объявления
        foreach (var item in items)
        {
            item.Files = await GetFilesAsync(item.Id, cancellationToken);
        }

        return new PagedResult<AdvertisementDto>
        {
            Items = items,
            TotalCount = total,
            Page = page.Page,
            PageSize = page.PageSize
        };
    }

    /// <summary>
    /// Получить список файлов, прикрепленных к объявлению.
    /// </summary>
    /// <param name="advertisementId">Идентификатор объявления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список файлов, отсортированный по порядку отображения.</returns>
    private async Task<List<FileInfoDto>> GetFilesAsync(Guid advertisementId, CancellationToken cancellationToken)
    {
        return await _context.AdvertisementFiles
            .AsNoTracking()
            .Where(af => af.AdvertisementId == advertisementId)
            .Join(
                _context.Files,
                af => af.FileId,
                f => f.Id,
                (af, f) => new { af, f })
            .OrderBy(x => x.af.Order)
            .Select(x => new FileInfoDto
            {
                Id = x.f.Id,
                FileName = x.f.FileName,
                Url = "/uploads/" + x.f.FilePath,
                ContentType = x.f.ContentType,
                FileSize = x.f.FileSize,
                CreatedAt = x.f.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<PagedResult<AdvertisementDto>> GetByAuthorAsync(
        Guid authorId,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Advertisements
            .Where(a => a.AuthorId == authorId)
            .OrderByDescending(a => a.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
    
        var items = await query
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .Select(a => new AdvertisementDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Price = a.Price,
                CategoryId = a.CategoryId,
                AuthorId = a.AuthorId,
                Contact = new ContactDto
                {
                    Name = a.Contact.Name,
                    Email = a.Contact.Email,
                    Phone = a.Contact.Phone
                },
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdvertisementDto>
        {
            Items = items,
            TotalCount = total,
            Page = page.Page,
            PageSize = page.PageSize
        };
    }
    
    /// <inheritdoc />
    public async Task<PagedResult<AdvertisementDto>> SearchAsync(
        AdvertisementFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var predicate = AdvertisementPredicateBuilder.Build(filter);
    
        var query = _context.Advertisements
            .AsNoTracking()
            .Where(predicate);
    
        // Сортировка
        query = filter.SortBy switch
        {
            AdvertisementSortBy.CreatedAtDesc => query.OrderByDescending(a => a.CreatedAt),
            AdvertisementSortBy.CreatedAtAsc => query.OrderBy(a => a.CreatedAt),
            AdvertisementSortBy.PriceAsc => query.OrderBy(a => a.Price),
            AdvertisementSortBy.PriceDesc => query.OrderByDescending(a => a.Price),
            AdvertisementSortBy.TitleAsc => query.OrderBy(a => a.Title),
            AdvertisementSortBy.TitleDesc => query.OrderByDescending(a => a.Title),
            _ => query.OrderByDescending(a => a.CreatedAt)
        };
    
        var total = await query.CountAsync(cancellationToken);
    
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AdvertisementDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Price = a.Price,
                Status = (AdStatusDto)a.Status,
                CategoryId = a.CategoryId,
                AuthorId = a.AuthorId,
                Contact = new ContactDto
                {
                    Name = a.Contact.Name,
                    Email = a.Contact.Email,
                    Phone = a.Contact.Phone
                },
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);
    
        // Получаем категории отдельным запросом
        if (items.Any())
        {
            var categoryIds = items.Select(a => a.CategoryId).Distinct().ToList();
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
    
            foreach (var item in items)
            {
                item.CategoryName = categories.GetValueOrDefault(item.CategoryId);
                item.Files = await GetFilesAsync(item.Id, cancellationToken);
            }
        }
    
        return new PagedResult<AdvertisementDto>
        {
            Items = items,
            TotalCount = total,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }
}