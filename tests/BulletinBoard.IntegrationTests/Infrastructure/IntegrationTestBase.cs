using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using BulletinBoard.Application.Abstractions;
using BulletinBoard.Contracts.Auth;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Domain.Enums;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BulletinBoard.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<BulletinBoardWebApplicationFactory>, IAsyncLifetime
{
    protected readonly BulletinBoardWebApplicationFactory Factory;
    
    // ✅ ИСПРАВЛЕНО: _scope и DbContext теперь невидимы для тестов-наследников,
    // что предотвращает их неправильное использование.
    private IServiceScope _scope = null!;
    protected BulletinBoardDbContext DbContext = null!;

    protected IntegrationTestBase(BulletinBoardWebApplicationFactory factory)
    {
        Factory = factory;
    }

    // Логика Initialize/Dispose теперь идеальна для стратегии "уникальная БД на тест".
    public virtual async Task InitializeAsync()
    {
        _scope = Factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<BulletinBoardDbContext>();
        // EnsureCreatedAsync достаточно, так как имя БД всегда уникально.
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    public virtual Task DisposeAsync()
    {
        _scope?.Dispose();
        return Task.CompletedTask;
    }
    
    protected HttpClient CreateClient() => Factory.CreateClient();

    protected HttpClient CreateAuthorizedClient(string token)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected async Task<(string Token, Guid UserId)> AuthenticateAsUserAsync(HttpClient client)
    {
        var dto = new RegisterDto
        {
            DisplayName = "Test User",
            Email = $"user-{Guid.NewGuid()}@example.com",
            Password = "UserPassword123!"
        };
        return await AuthenticateInternalAsync(client, dto);
    }

    protected async Task<(string Token, Guid UserId)> AuthenticateAsAdminAsync(HttpClient client)
    {
        var adminEmail = $"admin-{Guid.NewGuid()}@example.com";
        var adminPassword = "AdminPassword123!";

        // ✅ ИСПРАВЛЕНО: Получаем сервисы из _scope, который принадлежит текущему тесту.
        var passwordHasher = _scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var hashedPassword = passwordHasher.HashPassword(adminPassword);
        
        var user = new User("Admin User", adminEmail, hashedPassword, null, UserRole.Admin);
        // ✅ ИСПРАВЛЕНО: Используем DbContext, который принадлежит текущему тесту.
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var loginDto = new LoginDto { Email = adminEmail, Password = adminPassword };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
    
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(loginResult!.AccessToken) as JwtSecurityToken;
        var userIdClaim = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(userIdClaim!.Value);

        return (loginResult.AccessToken, userId);
    }

    private async Task<(string Token, Guid UserId)> AuthenticateInternalAsync(HttpClient client, RegisterDto dto)
    {
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", dto);
        registerResponse.EnsureSuccessStatusCode();
        
        // ВАЖНО: После регистрации нам нужно залогиниться, чтобы получить токены.
        // Ваш предыдущий код пытался получить результат из регистрации, но регистрация
        // обычно возвращает 201 Created без тела с токенами.
        var loginDto = new LoginDto { Email = dto.Email, Password = dto.Password };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(loginResult!.AccessToken) as JwtSecurityToken;
        var userIdClaim = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(userIdClaim!.Value);

        return (loginResult.AccessToken, userId);
    }
}