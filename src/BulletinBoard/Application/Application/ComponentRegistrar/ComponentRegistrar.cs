using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Application.Contexts.Users;
using Microsoft.Extensions.DependencyInjection;

namespace BulletinBoard.Application.ComponentRegistrar;

/// <summary>
/// Точка регистрации прикладных сервисов.
/// </summary>
public static class ComponentRegistrar
{
    /// <summary>
    /// Регистрирует сервисы Application-уровня в контейнере DI.
    /// </summary>
    /// <param name="services">Контейнер зависимостей.</param>
    /// <returns>Тот же контейнер для чейнинга.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAdvertisementService, AdvertisementService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}