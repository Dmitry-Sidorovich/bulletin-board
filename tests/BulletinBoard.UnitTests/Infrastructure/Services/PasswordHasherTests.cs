using BulletinBoard.Infrastructure.Services.Auth;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Infrastructure.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_WithValidPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _hasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password); // Хеш не должен совпадать с исходным паролем
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Arrange
        var password = "SamePassword123";

        // Act
        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2); // BCrypt использует соль, поэтому хеши разные
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HashPassword_WithInvalidPassword_ThrowsArgumentException(string? invalidPassword)
    {
        // Act
        Action act = () => _hasher.HashPassword(invalidPassword!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Пароль не может быть пустым.*");
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "MySecretPassword";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var correctPassword = "CorrectPassword";
        var wrongPassword = "WrongPassword";
        var hash = _hasher.HashPassword(correctPassword);

        // Act
        var isValid = _hasher.VerifyPassword(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_SamePasswordDifferentHashes_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123";
        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);

        // Act & Assert
        // Хотя хеши разные, оба должны проходить проверку для одного пароля
        _hasher.VerifyPassword(password, hash1).Should().BeTrue();
        _hasher.VerifyPassword(password, hash2).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyPassword_WithInvalidPassword_ThrowsArgumentException(string? invalidPassword)
    {
        // Arrange
        var validHash = _hasher.HashPassword("SomePassword");

        // Act
        Action act = () => _hasher.VerifyPassword(invalidPassword!, validHash);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Пароль не может быть пустым.*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyPassword_WithInvalidHash_ThrowsArgumentException(string? invalidHash)
    {
        // Arrange
        var password = "SomePassword";

        // Act
        Action act = () => _hasher.VerifyPassword(password, invalidHash!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Хеш пароля не может быть пустым.*");
    }

    [Fact]
    public void HashPassword_ProducesBCryptFormattedHash()
    {
        // Arrange
        var password = "TestPassword";

        // Act
        var hash = _hasher.HashPassword(password);

        // Assert
        // BCrypt хеш начинается с "$2a$" или "$2b$" и имеет длину 60 символов
        hash.Should().StartWith("$2");
        hash.Should().HaveLength(60);
    }

    [Fact]
    public void VerifyPassword_IsCaseSensitive()
    {
        // Arrange
        var password = "CaseSensitive";
        var hash = _hasher.HashPassword(password);

        // Act
        var correctCase = _hasher.VerifyPassword("CaseSensitive", hash);
        var wrongCase = _hasher.VerifyPassword("casesensitive", hash);

        // Assert
        correctCase.Should().BeTrue();
        wrongCase.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_WithUnicodeCharacters_WorksCorrectly()
    {
        // Arrange
        var password = "Пароль123!@#";

        // Act
        var hash = _hasher.HashPassword(password);
        var isValid = _hasher.VerifyPassword(password, hash);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        isValid.Should().BeTrue();
    }
}
