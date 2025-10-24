using BulletinBoard.Contracts.Files;

namespace BulletinBoard.Application.Contexts.Files.Services;

/// <summary>
/// Сервис для работы с файлами (бизнес-логика).
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Загрузить файл.
    /// </summary>
    /// <param name="stream">Поток с данными файла.</param>
    /// <param name="fileName">Оригинальное имя файла.</param>
    /// <param name="contentType">MIME-тип файла.</param>
    /// <param name="fileSize">Размер файла в байтах.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат загрузки с ID и URL.</returns>
    Task<FileUploadResultDto> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить информацию о файле по ID.
    /// </summary>
    /// <param name="fileId">ID файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Информация о файле или null, если не найден.</returns>
    Task<FileInfoDto?> GetInfoAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Скачать файл.
    /// </summary>
    /// <param name="fileId">ID файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Кортеж (поток данных, имя файла, MIME-тип) или null.</returns>
    Task<(Stream Stream, string FileName, string ContentType)?> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить файл.
    /// </summary>
    /// <param name="fileId">ID файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>True, если файл удален; False, если не найден.</returns>
    Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
}