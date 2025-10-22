using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

Console.WriteLine("=== BulletinBoard Database Migrator ===\n");

// Читаем конфигурацию
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .Build();

var connectionString = configuration.GetConnectionString("MainDb")
                       ?? throw new InvalidOperationException("Connection string 'MainDb' not found in appsettings.json");

Console.WriteLine($"Database: {GetDatabaseName(connectionString)}\n");

// Создаем контекст напрямую (БЕЗ Host, БЕЗ DI)
var optionsBuilder = new DbContextOptionsBuilder<BulletinBoardDbContext>();
optionsBuilder.UseNpgsql(connectionString);

await using var context = new BulletinBoardDbContext(optionsBuilder.Options);

// Применяем миграции
Console.WriteLine("Applying migrations...");
await context.Database.MigrateAsync();

// Убедимся, что все таблицы созданы (на случай если миграции пустые)
Console.WriteLine("Ensuring database schema...");
await context.Database.EnsureCreatedAsync();

Console.WriteLine("✅ Migrations applied successfully!\n");

return;

static string GetDatabaseName(string connectionString)
{
    var parts = connectionString.Split(';');
    var dbPart = parts.FirstOrDefault(p => p.Trim().StartsWith("Database=", StringComparison.OrdinalIgnoreCase));
    return dbPart?.Split('=')[1].Trim() ?? "unknown";
}