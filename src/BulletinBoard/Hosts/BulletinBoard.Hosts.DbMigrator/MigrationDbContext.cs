using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Hosts.DbMigrator;

/// <summary>
/// DbContext, используемый ТОЛЬКО для генерации/применения миграций.
/// Наследует модель из <see cref="BulletinBoardDbContext"/> и не добавляет DbSet-ов.
/// </summary>
public sealed class MigrationDbContext : BulletinBoardDbContext
{
    /// <summary>
    /// Создаёт контекст миграций.
    /// </summary>
    /// <param name="options">
    /// Параметры EF Core для базового <see cref="BulletinBoardDbContext"/>.
    /// </param>
    public MigrationDbContext(DbContextOptions options)
        : base(options) { }
}