using BulletinBoard.Application.Validators.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Validators;

public class CreateAdvertisementDtoValidatorTests
{
    private readonly CreateAdvertisementDtoValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var dto = new CreateAdvertisementDto
        {
            Title = "Valid Title",
            Description = "Valid description",
            CategoryId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
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
        var dto = new CreateAdvertisementDto
        {
            Title = invalidTitle!,
            CategoryId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
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
        var dto = new CreateAdvertisementDto
        {
            Title = new string('A', 201), // MaxLength = 200
            CategoryId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
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
        var dto = new CreateAdvertisementDto
        {
            Title = "Valid Title",
            Description = new string('A', 1001), // MaxLength = 1000
            CategoryId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Description));
    }

    [Fact]
    public void Validate_WithEmptyCategoryId_ShouldFail()
    {
        // Arrange
        var dto = new CreateAdvertisementDto
        {
            Title = "Valid Title",
            CategoryId = Guid.Empty,
            AuthorId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.CategoryId));
    }

    [Fact]
    public void Validate_WithEmptyAuthorId_ShouldFail()
    {
        // Arrange
        var dto = new CreateAdvertisementDto
        {
            Title = "Valid Title",
            CategoryId = Guid.NewGuid(),
            AuthorId = Guid.Empty,
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.AuthorId));
    }

    [Fact]
    public void Validate_WithNullContact_ShouldFail()
    {
        // Arrange
        var dto = new CreateAdvertisementDto
        {
            Title = "Valid Title",
            CategoryId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Contact = null!
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Contact));
    }
}