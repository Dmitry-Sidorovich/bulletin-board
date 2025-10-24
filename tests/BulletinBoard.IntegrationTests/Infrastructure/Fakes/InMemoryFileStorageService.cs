using BulletinBoard.Application.Abstractions;

namespace BulletinBoard.IntegrationTests.Infrastructure.Fakes;

/// <summary>
/// Фейковая реализация файлового хранилища, работающая в памяти.
/// Для использования в интеграционных тестах.
/// </summary>
public class InMemoryFileStorageService : IFileStorageService
{
    // Используем словарь для имитации файловой системы: "путь" -> "содержимое"
    private readonly Dictionary<string, byte[]> _storage = new();

    public Task<string> SaveFileAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        // Генерируем уникальный "путь" как в реальном сервисе
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        _storage[uniqueFileName] = memoryStream.ToArray();
        
        // Возвращаем только имя файла, как и в реальном сервисе
        return Task.FromResult(uniqueFileName);
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // Просто удаляем из словаря. Не бросаем ошибку, если файла нет.
        _storage.Remove(filePath);
        return Task.CompletedTask;
    }

    public string GetFileUrl(string filePath)
    {
        // Возвращаем фейковый URL
        return $"/uploads/{filePath}";
    }

    public Task<Stream?> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (_storage.TryGetValue(filePath, out var bytes))
        {
            return Task.FromResult<Stream?>(new MemoryStream(bytes));
        }
        return Task.FromResult<Stream?>(null);
    }
}