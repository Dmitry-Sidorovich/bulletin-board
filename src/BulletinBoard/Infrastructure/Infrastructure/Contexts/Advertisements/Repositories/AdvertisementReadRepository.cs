using AutoMapper;
using AutoMapper.QueryableExtensions;
using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Contracts.Files;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Infrastructure.Contexts.Advertisements;

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
}