using BulletinBoard.Hosts.Api;                // CHANGED
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BulletinBoard.IntegrationTests.Infrastructure;

/// <summary>
/// Фабрика для создания тестового веб-приложения с InMemory БД и тестовым конфигом.
/// </summary>
public class BulletinBoardWebApplicationFactory : WebApplicationFactory<Program> // CHANGED
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // CHANGED: добавляем нужные ключи конфигурации, чтобы стартап не падал
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"]  = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:Secret"]   = "test-secret-min-32-chars-1234567890abcd", // >= 32 символов
                ["Jwt:AccessTokenExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"]   = "7"
            };
            config.AddInMemoryCollection(overrides);
        });
        
        builder.ConfigureLogging(loggingBuilder =>
        {
            // Полностью очищаем все провайдеры логирования, которые были
            // настроены в Program.cs (включая Serilog, пишущий в файлы/консоль).
            loggingBuilder.ClearProviders();
        });

        builder.ConfigureServices(services =>
        {
            // CHANGED: вырезаем настоящий DbContext
            var toRemove = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<BulletinBoardDbContext>) ||
                    d.ServiceType == typeof(BulletinBoardDbContext))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            // CHANGED: InMemory БД
            services.AddDbContext<BulletinBoardDbContext>(o =>
                // ИСПРАВЛЕНО: Используем одно и то же имя для всех тестов
                o.UseInMemoryDatabase("TestDb")); 
            
            services.AddSerilog(config =>
            {
                config
                    .MinimumLevel.Debug() // Включаем подробные логи для отладки тестов
                    .WriteTo.Console();   // Пишем логи в стандартный вывод консоли теста
            });


            // CHANGED: создаём БД
            // var sp = services.BuildServiceProvider();
            // using var scope = sp.CreateScope();
            // var db = scope.ServiceProvider.GetRequiredService<BulletinBoardDbContext>();
            // db.Database.EnsureCreated();
        });
    }
}
