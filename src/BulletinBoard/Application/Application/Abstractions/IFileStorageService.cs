namespace BulletinBoard.Application.Abstractions;

/// <summary>
/// Сервис для физического хранения файлов.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Сохранить файл на диск.
    /// </summary>
    /// <param name="stream">Поток с данными файла.</param>
    /// <param name="fileName">Оригинальное имя файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Относительный путь к сохраненному файлу (например: "abc123.jpg").</returns>
    Task<string> SaveFileAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить файл с диска.
    /// </summary>
    /// <param name="filePath">Относительный путь к файлу.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить публичный URL файла.
    /// </summary>
    /// <param name="filePath">Относительный путь к файлу.</param>
    /// <returns>Публичный URL (например: "/uploads/abc123.jpg").</returns>
    string GetFileUrl(string filePath);

    /// <summary>
    /// Прочитать файл с диска.
    /// </summary>
    /// <param name="filePath">Относительный путь к файлу.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Поток с данными файла или null, если файл не найден.</returns>
    Task<Stream?> ReadFileAsync(string filePath, CancellationToken cancellationToken = default);
}