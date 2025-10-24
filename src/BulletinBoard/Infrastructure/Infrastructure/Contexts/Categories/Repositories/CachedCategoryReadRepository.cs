using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Contracts.Categories;
using BulletinBoard.Contracts.Common;
using Microsoft.Extensions.Configuration;

namespace BulletinBoard.Infrastructure.Contexts.Categories.Repositories;

/// <summary>
/// Декоратор для кеширования чтения категорий.
/// </summary>
public class CachedCategoryReadRepository : ICategoryReadRepository
{
    private readonly ICategoryReadRepository _inner;
    private readonly ICacheService _cache;
    private readonly IConfiguration _config;
    
    private readonly TimeSpan _listTtl;

    /// <summary>
    /// Инициализирует декоратор с кешированием.
    /// </summary>
    /// <param name="inner">Оригинальный репозиторий.</param>
    /// <param name="cache">Сервис кеширования.</param>
    /// <param name="config">Конфигурация приложения.</param>
    public CachedCategoryReadRepository(
        ICategoryReadRepository inner,
        ICacheService cache,
        IConfiguration config)
    {
        _inner = inner;
        _cache = cache;
        _config = config;
        
        _listTtl = TimeSpan.FromMinutes(_config.GetValue<int>("Caching:CategoryListMinutes"));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryDto>> GetRootsAsync(CancellationToken ct = default)
    {
        const string key = "categories:root";

        // Проверяем кеш (кешируем как List, возвращаем как IReadOnlyList)
        var cached = await _cache.GetAsync<List<CategoryDto>>(key, ct);
        if (cached != null)
        {
            return cached;
        }

        // Запрос в БД
        var result = await _inner.GetRootsAsync(ct);
        
        // Сохраняем в кеш (конвертируем в List для сериализации)
        var listResult = result.ToList();
        await _cache.SetAsync(key, listResult, _listTtl, ct);

        return result;
    }

    /// <inheritdoc />
    public Task<PagedResult<CategoryDto>> GetChildrenAsync(
        Guid parentId,
        PageRequest page,
        CancellationToken ct = default)
    {
        return _inner.GetChildrenAsync(parentId, page, ct);
    }
    
    /// <inheritdoc />
    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = $"category:{id}";
        // Отдельные сущности можно кешировать дольше, чем списки.
        // Для этого можно добавить отдельный TTL в конфиг, например, "CategoryDetailMinutes".
        // Для простоты пока используем тот же TTL, что и для списков.
        var ttl = _listTtl; 

        var cached = await _cache.GetAsync<CategoryDto>(key, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var result = await _inner.GetByIdAsync(id, cancellationToken);
        if (result != null)
        {
            await _cache.SetAsync(key, result, ttl, cancellationToken);
        }

        return result;
    }
}