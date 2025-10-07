using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Infrastructure.Contexts.Advertisements;

/// <inheritdoc />
public sealed class AdvertisementRepository : IAdvertisementRepository
{
    /// <inheritdoc />
    public Task<Advertisement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task AddAsync(Advertisement ad, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task UpdateAsync(Advertisement ad, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}