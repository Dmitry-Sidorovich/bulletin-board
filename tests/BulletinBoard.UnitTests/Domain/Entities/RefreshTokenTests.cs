using BulletinBoard.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Domain.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid_token_string";
        var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var refreshToken = new RefreshToken(userId, token, expiresAt);

        // Assert
        refreshToken.UserId.Should().Be(userId);
        refreshToken.Token.Should().Be(token);
        refreshToken.ExpiresAt.Should().Be(expiresAt);
        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new RefreshToken(Guid.Empty, "token", DateTimeOffset.UtcNow.AddDays(7));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("userId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidToken_ShouldThrowArgumentException(string? invalidToken)
    {
        // Arrange & Act
        var act = () => new RefreshToken(Guid.NewGuid(), invalidToken!, DateTimeOffset.UtcNow.AddDays(7));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("token");
    }

    [Fact]
    public void Constructor_WithPastExpiryDate_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new RefreshToken(Guid.NewGuid(), "token", DateTimeOffset.UtcNow.AddDays(-1));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("expiresAt")
            .WithMessage("*должна быть в будущем*");
    }

    [Fact]
    public void Revoke_ShouldSetIsRevokedToTrue()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token", DateTimeOffset.UtcNow.AddDays(7));
        refreshToken.IsRevoked.Should().BeFalse();

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token", DateTimeOffset.UtcNow.AddDays(7));
        refreshToken.Revoke();

        // Act
        var act = () => refreshToken.Revoke();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*уже отозван*");
    }

    [Fact]
    public void IsActive_WhenNotExpiredAndNotRevoked_ShouldReturnTrue()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token", DateTimeOffset.UtcNow.AddDays(7));

        // Act & Assert
        refreshToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), "token", DateTimeOffset.UtcNow.AddDays(7));
        refreshToken.Revoke();

        // Act & Assert
        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        // ✅ Создаём токен, который истечёт через 1 миллисекунду
        var refreshToken = new RefreshToken(
            Guid.NewGuid(), 
            "token", 
            DateTimeOffset.UtcNow.AddMilliseconds(1));

        // Act
        // Ждём, чтобы токен истёк
        Thread.Sleep(10);

        // Assert
        refreshToken.IsActive.Should().BeFalse();
    }
}