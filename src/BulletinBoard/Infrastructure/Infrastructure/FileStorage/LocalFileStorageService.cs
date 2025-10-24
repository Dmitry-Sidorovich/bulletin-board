using BulletinBoard.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;

namespace BulletinBoard.Infrastructure.FileStorage;

/// <summary>
/// Локальное хранилище файлов (wwwroot/uploads/).
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private const string BaseUrl = "/uploads";

    /// <summary>
    /// Инициализирует локальное файловое хранилище.
    /// </summary>
    /// <param name="environment">Окружение веб-приложения.</param>
    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _uploadPath = Path.Combine(environment.WebRootPath, "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    /// <inheritdoc />
    public async Task<string> SaveFileAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_uploadPath, uniqueFileName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);

        return uniqueFileName;
    }

    /// <inheritdoc />
    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_uploadPath, filePath);

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public string GetFileUrl(string filePath)
    {
        return $"{BaseUrl}/{filePath}";
    }

    /// <inheritdoc />
    public Task<Stream?> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_uploadPath, filePath);

        if (!System.IO.File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream?>(stream);
    }
}