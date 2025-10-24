using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Files.Repositories;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;
using File = BulletinBoard.Domain.Entities.File;

namespace BulletinBoard.Infrastructure.Contexts.Files.Repositories;

/// <inheritdoc />
public sealed class FileRepository : IFileRepository
{
    private readonly BulletinBoardDbContext _context;
    private readonly IFileStorageService _storageService;

    /// <summary>
    /// Инициализирует репозиторий файлов.
    /// </summary>
    public FileRepository(BulletinBoardDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    /// <inheritdoc />
    public async Task AddAsync(File file, CancellationToken cancellationToken = default)
    {
        await _context.Files.AddAsync(file, cancellationToken);
    }

    /// <inheritdoc />
    public Task<File?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return _context.Files
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var fileToDelete = await _context.Files.FindAsync(new object[] { fileId }, cancellationToken);
        if (fileToDelete != null)
        {
            // Удаляем файл из хранилища
            await _storageService.DeleteFileAsync(fileToDelete.FilePath, cancellationToken);

            // Удаляем запись из БД
            _context.Files.Remove(fileToDelete);
        }
    }
}