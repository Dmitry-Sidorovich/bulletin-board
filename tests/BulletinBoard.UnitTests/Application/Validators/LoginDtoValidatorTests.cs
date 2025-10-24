using BulletinBoard.Application.Validators.Auth;
using BulletinBoard.Contracts.Auth;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Validators;

public class LoginDtoValidatorTests
{
    private readonly LoginDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "john@example.com",
            Password = "Password123"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void Validate_WithInvalidEmail_ShouldFail(string? invalidEmail)
    {
        // Arrange
        var dto = new LoginDto
        {
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
    [InlineData("   ")]
    public void Validate_WithInvalidPassword_ShouldFail(string? invalidPassword)
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "john@test.com",
            Password = invalidPassword!
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Password));
    }
}