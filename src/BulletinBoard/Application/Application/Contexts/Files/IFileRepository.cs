using File = BulletinBoard.Domain.Entities.File;

namespace BulletinBoard.Application.Contexts.Files.Repositories;

/// <summary>
/// Репозиторий для работы с файлами.
/// </summary>
public interface IFileRepository
{
    /// <summary>
    /// Добавить файл в БД.
    /// </summary>
    Task AddAsync(File file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить файл по ID.
    /// </summary>
    Task<File?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить файл из БД.
    /// </summary>
    Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
}