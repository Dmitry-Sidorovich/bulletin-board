using System.Text.Json;
using BulletinBoard.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BulletinBoard.Infrastructure.Services;

/// <summary>
/// Hybrid кеш: L1 (Memory) + L2 (Redis).
/// </summary>
public class HybridCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<HybridCacheService> _logger;
    private readonly double _memoryRatio;
    private readonly double _rehydrationMinutes;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса <see cref="HybridCacheService"/>.
    /// </summary>
    /// <param name="memoryCache">Провайдер L1 кеша (в памяти).</param>
    /// <param name="distributedCache">Провайдер L2 распределенного кеша (например, Redis).</param>
    /// <param name="logger">Сервис для логирования операций кеширования.</param>
    /// <param name="configuration">Провайдер конфигурации для доступа к настройкам приложения, таким как 'Caching:MemoryCacheRatio'.</param>
    public HybridCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<HybridCacheService> logger,
        IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        _memoryRatio = configuration.GetValue<double>("Caching:MemoryCacheRatio");
        _rehydrationMinutes = configuration.GetValue<double>("Caching:MemoryRehydrationMinutes");
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        // 1. Проверяем Memory
        if (_memoryCache.TryGetValue(key, out T? cached))
        {
            _logger.LogDebug("Cache HIT (Memory): {Key}", key);
            return cached;
        }

        // 2. Проверяем Redis
        try
        {
            var redisValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (!string.IsNullOrEmpty(redisValue))
            {
                _logger.LogDebug("Cache HIT (Redis): {Key}", key);
                var value = JsonSerializer.Deserialize<T>(redisValue);

                // Подогрев Memory: кладём на фиксированное время
                if (value != null)
                {
                    var memoryTtl = TimeSpan.FromMinutes(_rehydrationMinutes);
                    _memoryCache.Set(key, value, memoryTtl);
                }

                return value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GET failed: {Key}", key);
        }

        _logger.LogDebug("Cache MISS: {Key}", key);
        return null;
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default) where T : class
    {
        // 1. Memory: 30% от Redis TTL
        var memoryTtl = TimeSpan.FromMinutes(Math.Max(1, ttl.TotalMinutes * _memoryRatio));
        _memoryCache.Set(key, value, memoryTtl);
        _logger.LogDebug("Cache SET (Memory): {Key} ({Ttl:F1}m)", key, memoryTtl.TotalMinutes);

        // 2. Redis: полный TTL
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            }, cancellationToken);
            
            _logger.LogDebug("Cache SET (Redis): {Key} ({Ttl:F1}m)", key, ttl.TotalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SET failed: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Cache REMOVE: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis REMOVE failed: {Key}", key);
        }
    }
}