namespace BulletinBoard.Contracts.Files;

/// <summary>
/// Результат загрузки файла.
/// </summary>
public class FileUploadResultDto
{
    /// <summary>
    /// ID загруженного файла.
    /// </summary>
    public Guid FileId { get; init; }

    /// <summary>
    /// Публичный URL для доступа.
    /// </summary>
    public required string Url { get; init; }
}