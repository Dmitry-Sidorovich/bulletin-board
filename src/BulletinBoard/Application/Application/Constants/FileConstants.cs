namespace BulletinBoard.Application.Constants;

/// <summary>
/// Константы для работы с файлами.
/// </summary>
public static class FileConstants
{
    /// <summary>
    /// Максимальный размер файла в байтах (5 МБ).
    /// </summary>
    public const long MaxFileSize = 5 * 1024 * 1024;

    /// <summary>
    /// Максимальная длина имени файла.
    /// </summary>
    public const int MaxFileNameLength = 255;

    /// <summary>
    /// Допустимые расширения файлов.
    /// </summary>
    public const string AllowedExtensions = "jpg,jpeg,png,gif,pdf,doc,docx";
}
