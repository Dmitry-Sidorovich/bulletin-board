using BulletinBoard.Domain.Base;

namespace BulletinBoard.Domain.Entities;

/// <summary>
/// Файл в системе (изображение, документ).
/// Независимая сущность, может быть прикреплена к разным объектам.
/// </summary>
public class File : EntityBase
{
    /// <summary>
    /// Оригинальное имя файла (например, "photo.jpg").
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// Путь к файлу на диске (относительный: "abc123.jpg").
    /// </summary>
    public string FilePath { get; private set; } = string.Empty;

    /// <summary>
    /// MIME-тип файла (например, "image/jpeg").
    /// </summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>
    /// Размер файла в байтах.
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    /// Конструктор для EF Core.
    /// </summary>
    private File() { }

    /// <summary>
    /// Создает новый файл.
    /// </summary>
    /// <param name="fileName">Оригинальное имя файла.</param>
    /// <param name="filePath">Относительный путь к файлу.</param>
    /// <param name="contentType">MIME-тип.</param>
    /// <param name="fileSize">Размер в байтах.</param>
    public File(string fileName, string filePath, string contentType, long fileSize)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Имя файла обязательно.", nameof(fileName));
        }
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Путь к файлу обязателен.", nameof(filePath));
        }
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("MIME-тип обязателен.", nameof(contentType));
        }
        if (fileSize <= 0)
        {
            throw new ArgumentException("Размер файла должен быть больше 0.", nameof(fileSize));
        }

        FileName = fileName;
        FilePath = filePath;
        ContentType = contentType;
        FileSize = fileSize;
    }
}