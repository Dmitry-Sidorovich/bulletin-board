using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Infrastructure.Contexts.Advertisements;

/// <inheritdoc />
public sealed class AdvertisementRepository : IAdvertisementRepository
{
    private readonly BulletinBoardDbContext _dbContext;

    /// <summary>
    /// Инициализирует экземпляр <see cref="AdvertisementRepository"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public AdvertisementRepository(BulletinBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <inheritdoc />
    public async Task<Advertisement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Advertisements
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Advertisement ad, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ad);
        await _dbContext.Advertisements.AddAsync(ad, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(Advertisement ad, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ad);
        _dbContext.Advertisements.Update(ad);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ad = await GetByIdAsync(id, cancellationToken);
        if (ad != null)
        {
            _dbContext.Advertisements.Remove(ad);
        }
    }
}