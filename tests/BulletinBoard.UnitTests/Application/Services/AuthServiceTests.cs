using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Application.Exceptions;
using BulletinBoard.Contracts.Auth;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Infrastructure.Contexts.Auth.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BulletinBoard.UnitTests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration.Setup(c => c["Jwt:RefreshTokenExpirationDays"]).Returns("30");
        _mockConfiguration.Setup(c => c["Jwt:AccessTokenExpirationMinutes"]).Returns("15");

        _service = new Infrastructure.Contexts.Auth.Services.AuthService(
            _mockUserRepository.Object,
            _mockRefreshTokenRepository.Object,
            _mockPasswordHasher.Object,
            _mockJwtTokenService.Object,
            _mockUnitOfWork.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldRegisterUser()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            DisplayName = "John Doe",
            Email = "john@example.com",
            Password = "SecurePassword123",
            Phone = "+79991234567"
        };

        var hashedPassword = "hashed_password";
        var accessToken = "access_token";
        var refreshToken = "refresh_token";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(registerDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockPasswordHasher.Setup(h => h.HashPassword(registerDto.Password))
            .Returns(hashedPassword);
        
        _mockJwtTokenService
            .Setup(j => j.GenerateAccessToken(It.IsAny<User>()))
            .Returns(accessToken);
        _mockJwtTokenService
            .Setup(j => j.GenerateRefreshToken())
            .Returns(refreshToken);

        // Act
        var result = await _service.RegisterAsync(registerDto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(registerDto.Email);
        result.User.DisplayName.Should().Be(registerDto.DisplayName);
        
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRefreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ShouldThrowValidationException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            DisplayName = "John Doe",
            Email = "existing@example.com",
            Password = "Password123"
        };

        var existingUser = new User("Existing User", registerDto.Email, "hash");
        _mockUserRepository.Setup(r => r.GetByEmailAsync(registerDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var act = async () => await _service.RegisterAsync(registerDto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*существует*");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "Password123"
        };

        var user = new User("John Doe", loginDto.Email, "hashed_password");
        var accessToken = "access_token";
        var refreshToken = "refresh_token";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(loginDto.Password, user.PasswordHash))
            .Returns(true);
        
        _mockJwtTokenService
            .Setup(j => j.GenerateAccessToken(It.Is<User>(u => u.Email == user.Email)))
            .Returns(accessToken);
        _mockJwtTokenService
            .Setup(j => j.GenerateRefreshToken())
            .Returns(refreshToken);
        // Act
        var result = await _service.LoginAsync(loginDto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.User.Email.Should().Be(loginDto.Email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowValidationException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _service.LoginAsync(loginDto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*email*");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowValidationException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "WrongPassword"
        };

        var user = new User("John Doe", loginDto.Email, "hashed_password");

        _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPasswordHasher.Setup(h => h.VerifyPassword(loginDto.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var act = async () => await _service.LoginAsync(loginDto, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*пароль*");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var oldRefreshToken = "old_refresh_token";
        var userId = Guid.NewGuid();
        var user = new User("John Doe", "john@example.com", "hash");
        
        var storedRefreshToken = new RefreshToken(
            userId,
            oldRefreshToken,
            DateTimeOffset.UtcNow.AddDays(7));

        var newAccessToken = "new_access_token";
        var newRefreshToken = "new_refresh_token";

        _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(oldRefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedRefreshToken);
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    
        _mockJwtTokenService
            .Setup(j => j.GenerateAccessToken(It.Is<User>(u => u.Email == user.Email)))
            .Returns(newAccessToken);
        _mockJwtTokenService
            .Setup(j => j.GenerateRefreshToken())
            .Returns(newRefreshToken);

        // Act
        var result = await _service.RefreshTokenAsync(
            new RefreshTokenDto { RefreshToken = oldRefreshToken }, 
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(newAccessToken);
        result.RefreshToken.Should().Be(newRefreshToken);
    
        storedRefreshToken.IsRevoked.Should().BeTrue(); // ✅ Проверяем реальное свойство
        _mockRefreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ShouldThrowValidationException()
    {
        // Arrange
        var invalidToken = "invalid_token";
        _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(invalidToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var act = async () => await _service.RefreshTokenAsync(
            new RefreshTokenDto { RefreshToken = invalidToken }, 
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*недействительный*");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowValidationException()
    {
        // Arrange
        var expiredToken = "expired_token";
        var userId = Guid.NewGuid();
    
        var refreshToken = new RefreshToken(
            userId,
            expiredToken,
            DateTimeOffset.UtcNow.AddDays(-1)); // Истекший токен

        _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(expiredToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act
        var act = async () => await _service.RefreshTokenAsync(
            new RefreshTokenDto { RefreshToken = expiredToken }, 
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*недействительный*");
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_ShouldRevokeToken()
    {
        // Arrange
        var token = "valid_token";
        var userId = Guid.NewGuid();
    
        var refreshToken = new RefreshToken(
            userId,
            token,
            DateTimeOffset.UtcNow.AddDays(7));

        _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        // Act
        await _service.LogoutAsync(token, CancellationToken.None);

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidToken_ShouldNotThrow()
    {
        // Arrange
        var token = "invalid_token";
        _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var act = async () => await _service.LogoutAsync(token, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}