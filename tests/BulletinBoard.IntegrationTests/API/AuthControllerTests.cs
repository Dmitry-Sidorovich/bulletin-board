using System.Net;
using System.Net.Http.Json;
using BulletinBoard.Contracts.Auth;
using BulletinBoard.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.IntegrationTests.API;

public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(BulletinBoardWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsTokens()
    {
        // Arrange
        var client = CreateClient();
        var registerDto = new RegisterDto
        {
            DisplayName = "John Doe",
            Email = "john.doe@example.com",
            Password = "SecurePassword123!",
            Phone = "+79991234567"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var registerDto = new RegisterDto
        {
            DisplayName = "John Doe",
            Email = "duplicate@example.com",
            Password = "SecurePassword123!"
        };
        // Первая регистрация
        await client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Act - попытка зарегистрировать того же пользователя
        var response = await client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var client = CreateClient();
        var registerDto = new RegisterDto
        {
            DisplayName = "John Doe",
            Email = "login.test@example.com",
            Password = "SecurePassword123!"
        };
        await client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new LoginDto
        {
            Email = "login.test@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var client = CreateClient();
        // 1. Регистрируемся и получаем первый набор токенов
        var registerDto = new RegisterDto
        {
            DisplayName = "Refresh Test",
            Email = "refresh@example.com",
            Password = "SecurePassword123!"
        };
        var initialResponse = await client.PostAsJsonAsync("/api/auth/register", registerDto);
        initialResponse.EnsureSuccessStatusCode();
        var initialTokens = await initialResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
    
        // 2. Извлекаем refresh token
        var oldRefreshToken = initialTokens!.RefreshToken;
        var refreshDto = new RefreshTokenDto { RefreshToken = oldRefreshToken };

        // Act
        // 3. Используем полученный refresh token для обновления
        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", refreshDto);

        // Assert
        // 4. Проверяем, что получили новые токены, и они отличаются от старых
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = await refreshResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
    
        newTokens.Should().NotBeNull();
        newTokens!.AccessToken.Should().NotBeNullOrEmpty().And.NotBe(initialTokens.AccessToken);
        newTokens.RefreshToken.Should().NotBeNullOrEmpty().And.NotBe(oldRefreshToken);
    }

    [Fact]
    public async Task Register_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var client = CreateClient();
        var invalidDto = new RegisterDto
        {
            DisplayName = "a", // Слишком короткое
            Email = "invalid-email", // Невалидный email
            Password = "123" // Слишком короткий пароль
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}