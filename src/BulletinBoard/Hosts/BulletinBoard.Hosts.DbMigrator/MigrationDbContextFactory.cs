using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BulletinBoard.Hosts.DbMigrator;

/// <summary>
/// Design-time фабрика, которую вызывает EF CLI (dotnet-ef),
/// чтобы создать <see cref="MigrationDbContext"/> без Host/DI.
/// </summary>
public sealed class MigrationDbContextFactory : IDesignTimeDbContextFactory<MigrationDbContext>
{
    /// <summary>
    /// Создаёт контекст для утилиты миграций.
    /// </summary>
    /// <param name="args">Аргументы CLI (не используются).</param>
    public MigrationDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        var cs = configuration.GetConnectionString("MainDb")
                 ?? throw new InvalidOperationException("ConnectionStrings:MainDb is missing.");

        var options = new DbContextOptionsBuilder<MigrationDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new MigrationDbContext(options);
    }
}