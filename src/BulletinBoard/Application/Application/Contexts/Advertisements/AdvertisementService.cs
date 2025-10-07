using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;

namespace BulletinBoard.Application.Contexts.Advertisements;

/// <inheritdoc />
public sealed class AdvertisementService : IAdvertisementService
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

    /// <inheritdoc />
    public Task<AdvertisementDto> CreateAsync(CreateAdvertisementDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc />
    public Task<AdvertisementDto?> UpdateAsync(Guid id, UpdateAdvertisementDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc />
    public Task<bool> ChangeStatusAsync(Guid id, ChangeAdvertisementStatusDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}