using BulletinBoard.Application.Validators.Categories;
using BulletinBoard.Contracts.Categories;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Validators;

public class CreateCategoryDtoValidatorTests
{
    private readonly CreateCategoryDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = "Electronics" };

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
        var dto = new CreateCategoryDto { Name = invalidName! };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Name));
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldFail()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = new string('A', 101) }; // MaxLength = 100

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Name));
    }
}