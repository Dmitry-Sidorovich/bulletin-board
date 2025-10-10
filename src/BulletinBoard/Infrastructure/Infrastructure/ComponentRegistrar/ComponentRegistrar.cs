using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Infrastructure.Contexts.Advertisements;
using BulletinBoard.Infrastructure.Contexts.Categories;
using BulletinBoard.Infrastructure.Contexts.Categories.Repositories;
using BulletinBoard.Infrastructure.Contexts.Users.Repositories;
using BulletinBoard.Infrastructure.DataAccess;
using BulletinBoard.Infrastructure.DataAccess.Db;
using BulletinBoard.Infrastructure.Mapping.Profiles;
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

        // Профили AutoMapper из сборки Infrastructure (Mapping/Profiles/*).
        services.AddAutoMapper(typeof(AdvertisementProfile).Assembly);

        // Read-репозитории (DTO/ProjectTo)
        services.AddScoped<IAdvertisementReadRepository, AdvertisementReadRepository>();
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();

        // Write-репозитории (домен)
        services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}