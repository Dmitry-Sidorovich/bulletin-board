using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Infrastructure.Contexts.Advertisements;

/// <inheritdoc />
public sealed class AdvertisementFileRepository : IAdvertisementFileRepository
{
    private readonly BulletinBoardDbContext _context;

    /// <summary>
    /// Инициализирует репозиторий связей объявлений и файлов.
    /// </summary>
    public AdvertisementFileRepository(BulletinBoardDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<bool> AdvertisementExistsAsync(Guid advertisementId, CancellationToken cancellationToken = default)
    {
        return _context.Advertisements
            .AsNoTracking()
            .AnyAsync(a => a.Id == advertisementId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> FileExistsAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return _context.Files
            .AsNoTracking()
            .AnyAsync(f => f.Id == fileId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> IsFileAttachedAsync(Guid advertisementId, Guid fileId, CancellationToken cancellationToken = default)
    {
        return _context.AdvertisementFiles
            .AsNoTracking()
            .AnyAsync(af => af.AdvertisementId == advertisementId && af.FileId == fileId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetMaxOrderAsync(Guid advertisementId, CancellationToken cancellationToken = default)
    {
        var maxOrder = await _context.AdvertisementFiles
            .AsNoTracking()
            .Where(af => af.AdvertisementId == advertisementId)
            .MaxAsync(af => (int?)af.Order, cancellationToken);

        return maxOrder ?? -1;
    }

    /// <inheritdoc />
    public async Task AddAsync(AdvertisementFile advertisementFile, CancellationToken cancellationToken = default)
    {
        await _context.AdvertisementFiles.AddAsync(advertisementFile, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid advertisementId, Guid fileId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.AdvertisementFiles
            .FirstOrDefaultAsync(af => af.AdvertisementId == advertisementId && af.FileId == fileId, cancellationToken);

        if (entity == null)
            return false;

        _context.AdvertisementFiles.Remove(entity);
        return true;
    }
}