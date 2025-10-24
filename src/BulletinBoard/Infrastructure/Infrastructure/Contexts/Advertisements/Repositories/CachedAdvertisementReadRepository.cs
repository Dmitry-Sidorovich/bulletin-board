using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;
using Microsoft.Extensions.Configuration;

namespace BulletinBoard.Infrastructure.Contexts.Advertisements.Repositories;

/// <summary>
/// Декоратор для кеширования чтения объявлений (Decorator Pattern).
/// </summary>
/// <remarks>
/// Эта реализация использует гибридный подход к кешированию объявлений:
///
/// КЕШИРУЕТСЯ:
/// - GetByIdAsync() - Отдельное объявление кешируется с TTL из конфигурации (обычно 30 минут).
///   Причина: Часто запрашиваемые данные, низкая потребность в актуальности.
///   Ключ: "ad:{advertisementId}"
///   Инвалидация: При обновлении/удалении объявления в AdvertisementService.
///
/// НЕ КЕШИРУЕТСЯ:
/// - GetByCategoryAsync() - Список по категориям не кешируется, т.к. с пагинацией кеш-ключ становится сложным.
/// - GetByAuthorAsync() - Список объявлений автора не кешируется по той же причине (динамичные результаты).
/// - SearchAsync() - Поиск с множеством параметров фильтрации не кешируется (сложность кеш-ключа, низкие повторяемость запросов).
///
/// СТРАТЕГИЯ ИНВАЛИДАЦИИ:
/// - При UpdateAsync() и DeleteAsync() в AdvertisementService вызывается RemoveAsync("ad:{id}")
/// - Это гарантирует согласованность кеша с БД при изменениях
/// - Список объявлений БЕЗ инвалидации (каждый запрос всегда к БД для свежести)
/// </remarks>
public class CachedAdvertisementReadRepository : IAdvertisementReadRepository
{
    private readonly IAdvertisementReadRepository _inner;
    private readonly ICacheService _cache;
    private readonly IConfiguration _config;

    /// <summary>
    /// Инициализирует декоратор с кешированием.
    /// </summary>
    /// <param name="inner">Оригинальный репозиторий для чтения объявлений.</param>
    /// <param name="cache">Сервис гибридного кеширования (Memory + Redis).</param>
    /// <param name="config">Конфигурация приложения для получения TTL параметров.</param>
    public CachedAdvertisementReadRepository(
        IAdvertisementReadRepository inner,
        ICacheService cache,
        IConfiguration config)
    {
        _inner = inner;
        _cache = cache;
        _config = config;
    }

    /// <summary>
    /// Получает объявление по ID с использованием кеша.
    /// </summary>
    /// <remarks>
    /// Процесс кеширования:
    /// 1. Проверяет наличие в кеше по ключу "ad:{id}"
    /// 2. Если найдено, возвращает из кеша (быстро)
    /// 3. Если нет, запрашивает из БД
    /// 4. Если результат не null, сохраняет в кеш с TTL из конфигурации
    /// </remarks>
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

    /// <summary>
    /// Получает пагинированный список объявлений по категории БЕЗ кеширования.
    /// </summary>
    /// <remarks>
    /// Не кешируется, так как:
    /// - Кеш-ключ должен включать номер страницы, размер страницы (сложность)
    /// - Низкая вероятность того, что один пользователь запросит одну и ту же страницу дважды
    /// - Список часто изменяется (новые объявления добавляются, старые архивируются)
    /// - Инвалидация кеша для каждого изменения потребовала бы удаления всех вариантов страниц
    /// </remarks>
    /// <inheritdoc />
    public Task<PagedResult<AdvertisementDto>> GetByCategoryAsync(
        Guid categoryId,
        PageRequest page,
        CancellationToken ct = default)
    {
        return _inner.GetByCategoryAsync(categoryId, page, ct);
    }

    /// <summary>
    /// Получает пагинированный список объявлений автора БЕЗ кеширования.
    /// </summary>
    /// <remarks>
    /// Не кешируется по тем же причинам, что и GetByCategoryAsync (см. замечания выше).
    /// </remarks>
    /// <inheritdoc />
    public Task<PagedResult<AdvertisementDto>> GetByAuthorAsync(
        Guid authorId,
        PageRequest page,
        CancellationToken ct = default)
    {
        return _inner.GetByAuthorAsync(authorId, page, ct);
    }

    /// <summary>
    /// Выполняет поиск объявлений с фильтрацией, сортировкой и пагинацией БЕЗ кеширования.
    /// </summary>
    /// <remarks>
    /// Не кешируется, так как:
    /// - Множество комбинаций параметров фильтрации (query, categoryId, price range, dates, status, author)
    /// - Создание уникального кеш-ключа требует сериализации всех параметров (высокая сложность)
    /// - Поиск - это обычно одноразовые или редкие запросы (низкая вероятность повторений)
    /// - Список результатов быстро устаревает из-за постоянного добавления новых объявлений
    /// - Инвалидация такого кеша была бы очень дорогой операцией
    ///
    /// АЛЬТЕРНАТИВА: Если понадобится кеширование поиска, можно:
    /// - Использовать ElasticSearch вместо поиска в БД
    /// - Кешировать только популярные поисковые запросы
    /// - Использовать Redis для кеширования с автоматическим TTL (без явной инвалидации)
    /// </remarks>
    /// <inheritdoc />
    public Task<PagedResult<AdvertisementDto>> SearchAsync(
        AdvertisementFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        return _inner.SearchAsync(filter, cancellationToken);
    }
}