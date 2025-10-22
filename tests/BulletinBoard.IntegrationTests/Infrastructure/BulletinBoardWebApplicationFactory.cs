using BulletinBoard.Application.Abstractions;
using BulletinBoard.Hosts.Api;
using BulletinBoard.Infrastructure.DataAccess.Db;
using BulletinBoard.IntegrationTests.Infrastructure.Fakes;
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
public class BulletinBoardWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"]  = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:Secret"]   = "test-secret-min-32-chars-1234567890abcd",
                ["Jwt:AccessTokenExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"]   = "7"
            };
            config.AddInMemoryCollection(overrides);
        });
        
        builder.ConfigureLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
        });

        builder.ConfigureServices(services =>
        {
            var toRemove = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<BulletinBoardDbContext>) ||
                    d.ServiceType == typeof(BulletinBoardDbContext))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);
            
            services.AddDbContext<BulletinBoardDbContext>(o =>
                o.UseInMemoryDatabase("TestDb")); 
            
            var serviceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IFileStorageService));
            if (serviceDescriptor != null)
            {
                services.Remove(serviceDescriptor);
            }
            
            services.AddSingleton<IFileStorageService, InMemoryFileStorageService>();
            
            services.AddSerilog(config =>
            {
                config
                    .MinimumLevel.Debug() 
                    .WriteTo.Console();
            });
        });
    }
}
