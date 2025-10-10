using BulletinBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Infrastructure.DataAccess.Db;

/// <summary>
/// EF Core контекст БД для приложения BulletinBoard.
/// </summary>
public class BulletinBoardDbContext : DbContext
{
    /// <summary>Набор категорий.</summary>
    public DbSet<Category> Categories => Set<Category>();
    /// <summary>Набор объявлений.</summary>
    public DbSet<Advertisement> Advertisements => Set<Advertisement>();
    /// <summary>Набор пользователей (авторов).</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Создаёт экземпляр контекста.
    /// </summary>
    public BulletinBoardDbContext(DbContextOptions options)
        : base(options) { }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BulletinBoardDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}