namespace BulletinBoard.Contracts.Files;

/// <summary>
/// Информация о файле (метаданные).
/// </summary>
public class FileInfoDto
{
    /// <summary>
    /// Идентификатор файла.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Оригинальное имя файла.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Публичный URL для доступа к файлу.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// MIME-тип файла.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Размер файла в байтах.
    /// </summary>
    public long FileSize { get; init; }

    /// <summary>
    /// Дата загрузки файла.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}