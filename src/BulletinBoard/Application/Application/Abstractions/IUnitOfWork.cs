namespace BulletinBoard.Application.Abstractions;

/// <summary>
/// Единица работы: централизует сохранение изменений в одной транзакции.
/// Репозитории не вызывают SaveChanges внутри.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Сохраняет накопленные изменения.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Количество затронутых записей.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}