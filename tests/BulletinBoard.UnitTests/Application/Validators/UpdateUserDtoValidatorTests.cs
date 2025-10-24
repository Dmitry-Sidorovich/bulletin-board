using BulletinBoard.Application.Contexts.Users.Validators;
using BulletinBoard.Contracts.Users;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Validators;

public class UpdateUserDtoValidatorTests
{
    private readonly UpdateUserDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var dto = new UpdateUserDto
        {
            DisplayName = "John Doe",
            Email = "john@example.com",
            Phone = "+79991234567"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullValues_ShouldPass()
    {
        // Arrange
        var dto = new UpdateUserDto
        {
            DisplayName = null,
            Email = null,
            Phone = null
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyDisplayName_ShouldFail(string emptyName)
    {
        // Arrange
        var dto = new UpdateUserDto { DisplayName = emptyName };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.DisplayName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void Validate_WithInvalidEmail_ShouldFail(string invalidEmail)
    {
        // Arrange
        var dto = new UpdateUserDto { Email = invalidEmail };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    public void Validate_WithInvalidPhone_ShouldFail(string invalidPhone)
    {
        // Arrange
        var dto = new UpdateUserDto { Phone = invalidPhone };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Phone));
    }
}