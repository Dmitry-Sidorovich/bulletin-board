namespace BulletinBoard.Application.Abstractions;

/// <summary>
/// Сервис для работы с кешем (L1: Memory, L2: Redis).
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Получает значение из кеша.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Сохраняет значение в кеш с TTL.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Удаляет значение из кеша.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}