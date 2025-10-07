using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Infrastructure.Contexts.Advertisements;
using BulletinBoard.Infrastructure.Contexts.Categories;
using BulletinBoard.Infrastructure.Contexts.Users.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BulletinBoard.Infrastructure.ComponentRegistrar;

/// <summary>
/// Расширения для регистрации зависимостей проекта.
/// </summary>
public static class ComponentRegistrar
{
    /// <summary>
    /// Регистрирует прикладные сервисы (Application).
    /// </summary>
    public static IServiceCollection RegisterAppServices(this IServiceCollection services)
    {
        // Сервисы (единые: чтение + команды)
        services.AddScoped<IAdvertisementService, AdvertisementService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }

    /// <summary>
    /// Регистрирует репозитории (write/read) — только интерфейсы и заглушки-реализации.
    /// </summary>
    public static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        // Advertisements
        services.AddScoped<IAdvertisementRepository, AdvertisementRepository>();
        services.AddScoped<IAdvertisementReadRepository, AdvertisementReadRepository>();

        // Categories
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();

        // Users
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();

        return services;
    }
}