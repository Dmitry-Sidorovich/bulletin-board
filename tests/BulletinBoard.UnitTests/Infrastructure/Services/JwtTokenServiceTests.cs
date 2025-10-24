using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Domain.Enums;
using BulletinBoard.Infrastructure.Services.Auth;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BulletinBoard.UnitTests.Infrastructure.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;
    private readonly IConfiguration _configuration;

    public JwtTokenServiceTests()
    {
        // Создаём тестовую конфигурацию с реальными значениями JWT
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "test-secret-key-minimum-32-characters-long-12345",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:AccessTokenExpirationMinutes"] = "60"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _service = new JwtTokenService(_configuration);
    }

    [Fact]
    public void GenerateAccessToken_CreatesValidToken()
    {
        // Arrange
        var user = new User(
            displayName: "Test User",
            email: "test@example.com",
            passwordHash: "hash",
            phone: null,
            role: UserRole.User);

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        // Проверяем, что токен можно распарсить
        var handler = new JwtSecurityTokenHandler();
        var canRead = handler.CanReadToken(token);
        canRead.Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_ContainsCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User(
            displayName: "John Doe",
            email: "john@example.com",
            passwordHash: "hash",
            phone: "+79991234567",
            role: UserRole.Admin);

        // Используем рефлексию чтобы установить ID (т.к. он readonly)
        var idField = typeof(User).BaseType!.GetField("<Id>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        idField!.SetValue(user, userId);

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Проверяем наличие всех необходимых claims
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "john@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectIssuerAndAudience()
    {
        // Arrange
        var user = new User("Test", "test@example.com", "hash", null, UserRole.User);

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectExpiration()
    {
        // Arrange
        var user = new User("Test", "test@example.com", "hash", null, UserRole.User);
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _service.GenerateAccessToken(user);
        var afterGeneration = DateTime.UtcNow;

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Токен должен истечь через 60 минут (из конфигурации)
        var expectedExpiration = beforeGeneration.AddMinutes(60);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        // Act
        var refreshToken = _service.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDifferentTokensEachTime()
    {
        // Act
        var token1 = _service.GenerateRefreshToken();
        var token2 = _service.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ReturnsUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("Test", "test@example.com", "hash", null, UserRole.User);

        // Устанавливаем ID через рефлексию
        var idField = typeof(User).BaseType!.GetField("<Id>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        idField!.SetValue(user, userId);

        var token = _service.GenerateAccessToken(user);

        // Act
        var extractedUserId = _service.GetUserIdFromToken(token);

        // Assert
        extractedUserId.Should().NotBeNull();
        extractedUserId.Should().Be(userId);
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var userId = _service.GetUserIdFromToken(invalidToken);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GetUserIdFromToken_WithExpiredToken_ReturnsNull()
    {
        // Arrange - создаём конфигурацию с очень коротким временем жизни токена
        var expiredConfig = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "test-secret-key-minimum-32-characters-long-12345",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:AccessTokenExpirationMinutes"] = "0" // Токен истекает мгновенно
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(expiredConfig)
            .Build();

        var serviceWithExpiredToken = new JwtTokenService(config);
        var user = new User("Test", "test@example.com", "hash", null, UserRole.User);
        var token = serviceWithExpiredToken.GenerateAccessToken(user);

        // Небольшая задержка, чтобы токен точно истёк
        System.Threading.Thread.Sleep(100);

        // Act
        var userId = _service.GetUserIdFromToken(token);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GetUserIdFromToken_WithTamperedSignature_ReturnsNull()
    {
        // Arrange
        var user = new User("Test", "test@example.com", "hash", null, UserRole.User);
        var token = _service.GenerateAccessToken(user);

        // Подделываем токен (меняем последние символы подписи)
        var tamperedToken = token.Substring(0, token.Length - 10) + "TAMPERED!!";

        // Act
        var userId = _service.GetUserIdFromToken(tamperedToken);

        // Assert
        userId.Should().BeNull();
    }
}
