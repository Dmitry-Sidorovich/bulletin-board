using BulletinBoard.Application.Abstractions;
using BulletinBoard.Infrastructure.DataAccess.Db;

namespace BulletinBoard.Infrastructure.DataAccess;

/// <summary>
/// Реализация Unit of Work через DbContext.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BulletinBoardDbContext _dbContext;

    /// <summary>
    /// Инициализирует экземпляр <see cref="UnitOfWork"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public UnitOfWork(BulletinBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}