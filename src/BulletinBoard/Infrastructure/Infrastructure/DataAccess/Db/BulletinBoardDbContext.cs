using BulletinBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using File = BulletinBoard.Domain.Entities.File;

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
    
    /// <summary>Набор файлов.</summary>
    public DbSet<File> Files => Set<File>();
    
    /// <summary>Набор связей между объявлениями и файлами.</summary>
    public DbSet<AdvertisementFile> AdvertisementFiles => Set<AdvertisementFile>();
    
    /// <summary>Набор refresh-токенов.</summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Создаёт экземпляр контекста.
    /// </summary>
    /// <param name="options">Опции конфигурации контекста.</param>
    public BulletinBoardDbContext(DbContextOptions options)
        : base(options) { }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BulletinBoardDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}