using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;
using Microsoft.Extensions.Configuration;

namespace BulletinBoard.Infrastructure.Contexts.Advertisements.Repositories;

/// <summary>
/// Декоратор для кеширования чтения объявлений.
/// </summary>
public class CachedAdvertisementReadRepository : IAdvertisementReadRepository
{
    private readonly IAdvertisementReadRepository _inner;
    private readonly ICacheService _cache;
    private readonly IConfiguration _config;

    /// <summary>
    /// Инициализирует декоратор с кешированием.
    /// </summary>
    /// <param name="inner">Оригинальный репозиторий.</param>
    /// <param name="cache">Сервис кеширования.</param>
    /// <param name="config">Конфигурация приложения.</param>
    public CachedAdvertisementReadRepository(
        IAdvertisementReadRepository inner,
        ICacheService cache,
        IConfiguration config)
    {
        _inner = inner;
        _cache = cache;
        _config = config;
    }

    /// <inheritdoc />
    public async Task<AdvertisementDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var key = $"ad:{id}";
        var ttl = TimeSpan.FromMinutes(_config.GetValue<int>("Caching:AdvertisementDetailMinutes"));
        
        var cached = await _cache.GetAsync<AdvertisementDto>(key, ct);
        if (cached != null)
        {
            return cached;
        }
        
        var result = await _inner.GetByIdAsync(id, ct);
        
        if (result != null)
        {
            await _cache.SetAsync(key, result, ttl, ct);
        }

        return result;
    }

    /// <inheritdoc />
    public Task<PagedResult<AdvertisementDto>> GetByCategoryAsync(
        Guid categoryId,
        PageRequest page,
        CancellationToken ct = default)
    {
        return _inner.GetByCategoryAsync(categoryId, page, ct);
    }

    /// <inheritdoc />
    public Task<PagedResult<AdvertisementDto>> GetByAuthorAsync(
        Guid authorId,
        PageRequest page,
        CancellationToken ct = default)
    {
        return _inner.GetByAuthorAsync(authorId, page, ct);
    }
}