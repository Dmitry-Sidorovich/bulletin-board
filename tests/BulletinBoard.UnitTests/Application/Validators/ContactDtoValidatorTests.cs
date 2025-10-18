using BulletinBoard.Application.Validators.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Validators;

public class ContactDtoValidatorTests
{
    private readonly ContactDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var dto = new ContactDto
        {
            Name = "John Doe",
            Email = "john@example.com",
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
    public void Validate_WithInvalidName_ShouldFail(string? invalidName)
    {
        // Arrange
        var dto = new ContactDto
        {
            Name = invalidName!,
            Email = "john@test.com"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Validate_WithInvalidEmail_ShouldFail(string? invalidEmail)
    {
        // Arrange
        var dto = new ContactDto
        {
            Name = "John Doe",
            Email = invalidEmail!
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Email));
    }

    [Fact]
    public void Validate_WithNullPhone_ShouldPass()
    {
        // Arrange
        var dto = new ContactDto
        {
            Name = "John Doe",
            Email = "john@test.com",
            Phone = null
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("+79991234567")]
    [InlineData("+1234567890")]
    [InlineData("+44123456789")]
    public void Validate_WithValidPhone_ShouldPass(string validPhone)
    {
        // Arrange
        var dto = new ContactDto
        {
            Name = "John Doe",
            Email = "john@test.com",
            Phone = validPhone
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("999-999-9999")]
    public void Validate_WithInvalidPhone_ShouldFail(string invalidPhone)
    {
        // Arrange
        var dto = new ContactDto
        {
            Name = "John Doe",
            Email = "john@test.com",
            Phone = invalidPhone
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Phone));
    }
}