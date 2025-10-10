using BulletinBoard.Application.Contexts.Files.Repositories;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;
using File = BulletinBoard.Domain.Entities.File;

namespace BulletinBoard.Infrastructure.Contexts.Files.Repositories;

/// <inheritdoc />
public sealed class FileRepository : IFileRepository
{
    private readonly BulletinBoardDbContext _context;

    /// <summary>
    /// Инициализирует репозиторий файлов.
    /// </summary>
    public FileRepository(BulletinBoardDbContext context)
    {
        _context = context;
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
    public Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return _context.Files
            .Where(f => f.Id == fileId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}