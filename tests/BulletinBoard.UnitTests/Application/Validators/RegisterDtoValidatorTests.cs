using BulletinBoard.Application.Validators.Auth;
using BulletinBoard.Contracts.Auth;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Validators;

public class RegisterDtoValidatorTests
{
    private readonly RegisterDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var dto = new RegisterDto
        {
            DisplayName = "John Doe",
            Email = "john@example.com",
            Password = "SecurePassword123",
            Phone = "+79991234567"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidDisplayName_ShouldFail(string? invalidName)
    {
        // Arrange
        var dto = new RegisterDto
        {
            DisplayName = invalidName!,
            Email = "john@test.com",
            Password = "Password123"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.DisplayName));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void Validate_WithDisplayNameTooShort_ShouldFail(string shortName)
    {
        // Arrange
        var dto = new RegisterDto
        {
            DisplayName = shortName,
            Email = "john@test.com",
            Password = "Password123"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.DisplayName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void Validate_WithInvalidEmail_ShouldFail(string? invalidEmail)
    {
        // Arrange
        var dto = new RegisterDto
        {
            DisplayName = "John Doe",
            Email = invalidEmail!,
            Password = "Password123"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    public void Validate_WithInvalidPassword_ShouldFail(string? invalidPassword)
    {
        // Arrange
        var dto = new RegisterDto
        {
            DisplayName = "John Doe",
            Email = "john@test.com",
            Password = invalidPassword!
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Password));
    }

    [Fact]
    public void Validate_WithNullPhone_ShouldPass()
    {
        // Arrange
        var dto = new RegisterDto
        {
            DisplayName = "John Doe",
            Email = "john@test.com",
            Password = "Password123",
            Phone = null
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}