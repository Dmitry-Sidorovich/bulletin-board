using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;

namespace BulletinBoard.Infrastructure.Contexts.Advertisements;

/// <inheritdoc />
public sealed class AdvertisementReadRepository : IAdvertisementReadRepository
{
    /// <inheritdoc />
    public Task<AdvertisementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<PagedResult<AdvertisementDto>> GetByCategoryAsync(Guid categoryId, PageRequest page, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}