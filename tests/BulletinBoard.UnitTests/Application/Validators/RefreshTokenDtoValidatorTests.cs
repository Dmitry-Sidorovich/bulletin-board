using BulletinBoard.Application.Validators.Auth;
using BulletinBoard.Contracts.Auth;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Validators;

public class RefreshTokenDtoValidatorTests
{
    private readonly RefreshTokenDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidRefreshToken_ShouldPass()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "valid-refresh-token-string-12345"
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
    public void Validate_WithEmptyRefreshToken_ShouldFail(string? invalidToken)
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = invalidToken!
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.RefreshToken));
    }

    [Fact]
    public void Validate_WithWhitespaceOnlyToken_ShouldFail()
    {
        // Arrange
        var dto = new RefreshTokenDto
        {
            RefreshToken = "     " // Только пробелы
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.RefreshToken));
    }
}
