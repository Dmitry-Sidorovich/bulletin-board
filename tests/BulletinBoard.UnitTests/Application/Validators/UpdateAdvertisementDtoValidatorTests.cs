using BulletinBoard.Application.Validators.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Validators;

public class UpdateAdvertisementDtoValidatorTests
{
    private readonly UpdateAdvertisementDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var dto = new UpdateAdvertisementDto
        {
            Title = "Valid Title",
            Description = "Valid description",
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto
            {
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "+79991234567"
            }
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
    public void Validate_WithInvalidTitle_ShouldFail(string? invalidTitle)
    {
        // Arrange
        var dto = new UpdateAdvertisementDto
        {
            Title = invalidTitle!,
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Title));
    }

    [Fact]
    public void Validate_WithTitleTooShort_ShouldFail()
    {
        // Arrange
        var dto = new UpdateAdvertisementDto
        {
            Title = "Ab", // MinLength = 3
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Title));
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldFail()
    {
        // Arrange
        var dto = new UpdateAdvertisementDto
        {
            Title = new string('A', 101), // MaxLength = 100
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Title));
    }

    [Fact]
    public void Validate_WithTitleContainingInvalidCharacters_ShouldFail()
    {
        // Arrange
        var dto = new UpdateAdvertisementDto
        {
            Title = "Invalid@Title#With$Symbols", // Не соответствует regex
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Title));
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var dto = new UpdateAdvertisementDto
        {
            Title = "Valid Title",
            Description = new string('A', 1001), // MaxLength = 1000
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Description));
    }

    [Fact]
    public void Validate_WithNullDescription_ShouldPass()
    {
        // Arrange
        var dto = new UpdateAdvertisementDto
        {
            Title = "Valid Title",
            Description = null, // Description опциональный
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyCategoryId_ShouldFail()
    {
        // Arrange
        var dto = new UpdateAdvertisementDto
        {
            Title = "Valid Title",
            CategoryId = Guid.Empty,
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.CategoryId));
    }

    [Fact]
    public void Validate_WithNullContact_ShouldFail()
    {
        // Arrange
        var dto = new UpdateAdvertisementDto
        {
            Title = "Valid Title",
            CategoryId = Guid.NewGuid(),
            Contact = null!
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Contact));
    }

    [Fact]
    public void Validate_WithInvalidContactEmail_ShouldFail()
    {
        // Arrange
        var dto = new UpdateAdvertisementDto
        {
            Title = "Valid Title",
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto
            {
                Name = "John",
                Email = "invalid-email" // Невалидный email
            }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Contact"));
    }
}
