using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Application.Contexts.Auth;
using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Application.Contexts.Files.Repositories;
using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Infrastructure.Contexts.Advertisements;
using BulletinBoard.Infrastructure.Contexts.Advertisements.Repositories;
using BulletinBoard.Infrastructure.Contexts.Auth.Repositories;
using BulletinBoard.Infrastructure.Contexts.Auth.Services;
using BulletinBoard.Infrastructure.Contexts.Categories;
using BulletinBoard.Infrastructure.Contexts.Categories.Repositories;
using BulletinBoard.Infrastructure.Contexts.Files.Repositories;
using BulletinBoard.Infrastructure.Contexts.Users.Repositories;
using BulletinBoard.Infrastructure.DataAccess;
using BulletinBoard.Infrastructure.DataAccess.Db;
using BulletinBoard.Infrastructure.FileStorage;
using BulletinBoard.Infrastructure.Mapping.Profiles;
using BulletinBoard.Infrastructure.Services;
using BulletinBoard.Infrastructure.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BulletinBoard.Infrastructure.ComponentRegistrar;

/// <summary>
/// Точка регистрации инфраструктуры: DbContext, AutoMapper, репозитории.
/// Реализации привязываются к абстракциям из Application.
/// </summary>
public static class ComponentRegistrar
{
    /// <summary>
    /// Регистрирует инфраструктуру (EF Core, профили маппинга и репозитории).
    /// </summary>
    /// <param name="services">Контейнер зависимостей.</param>
    /// <param name="configuration">Конфигурация (источник строки подключения и др.).</param>
    /// <returns>Тот же контейнер для чейнинга.</returns>
    /// <exception cref="InvalidOperationException">
    /// Выбрасывается, если отсутствует строка подключения <c>ConnectionStrings:MainDb</c>.
    /// </exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("MainDb")
                 ?? throw new InvalidOperationException("ConnectionStrings:MainDb is not configured.");

        services.AddDbContext<BulletinBoardDbContext>(opt => opt.UseNpgsql(cs));

        services.AddAutoMapper(typeof(AdvertisementProfile).Assembly);
        
        // Регистрируем оригинальную реализацию
        services.AddScoped<AdvertisementReadRepository>();
        // Регистрируем декоратор
        services.AddScoped<IAdvertisementReadRepository>(provider =>
        {
            var inner = provider.GetRequiredService<AdvertisementReadRepository>();
            var cache = provider.GetRequiredService<ICacheService>();
            var config = provider.GetRequiredService<IConfiguration>();
            return new CachedAdvertisementReadRepository(inner, cache, config);
        });
        
        services.AddScoped<CategoryReadRepository>();
        services.AddScoped<ICategoryReadRepository>(provider =>
        {
            var inner = provider.GetRequiredService<CategoryReadRepository>();
            var cache = provider.GetRequiredService<ICacheService>();
            var config = provider.GetRequiredService<IConfiguration>();
            return new CachedCategoryReadRepository(inner, cache, config);
        });
        
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IAdvertisementFileRepository, AdvertisementFileRepository>();
        
        services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddHttpContextAccessor();
        
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthService, AuthService>();
        
        services.AddMemoryCache();
        
        var enableRedis = configuration.GetValue<bool>("Caching:EnableRedis");
        if (enableRedis)
        {
            var redisConnection = configuration.GetConnectionString("Redis");
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "BulletinBoard:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }
        services.AddScoped<ICacheService, HybridCacheService>();

        return services;
    }
}