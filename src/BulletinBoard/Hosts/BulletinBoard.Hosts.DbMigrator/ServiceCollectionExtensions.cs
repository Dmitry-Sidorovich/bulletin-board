using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BulletinBoard.Hosts.DbMigrator;

/// <summary>
/// Расширения для регистрации сервисов в DI контейнере.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Регистрирует MigrationDbContext по ConnectionStrings:MainDb.</summary>
    public static IServiceCollection AddMigrationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("MainDb");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("ConnectionStrings:MainDb is missing or empty.");

        services.AddDbContext<MigrationDbContext>(opt => opt.UseNpgsql(cs));
        return services;
    }
}