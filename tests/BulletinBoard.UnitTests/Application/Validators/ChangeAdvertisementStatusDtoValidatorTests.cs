using BulletinBoard.Application.Validators.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Validators;

public class ChangeAdvertisementStatusDtoValidatorTests
{
    private readonly ChangeAdvertisementStatusDtoValidator _validator = new();

    [Theory]
    [InlineData(AdStatusDto.Draft)]
    [InlineData(AdStatusDto.Published)]
    [InlineData(AdStatusDto.Archived)]
    [InlineData(AdStatusDto.Blocked)]
    public void Validate_WithValidStatus_ShouldPass(AdStatusDto status)
    {
        // Arrange
        var dto = new ChangeAdvertisementStatusDto
        {
            StatusDto = status
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidStatusValue_ShouldFail()
    {
        // Arrange
        var dto = new ChangeAdvertisementStatusDto
        {
            StatusDto = (AdStatusDto)999 // Несуществующее значение enum
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.StatusDto));
    }

    [Fact]
    public void Validate_WithDefaultEnumValue_ShouldPass()
    {
        // Arrange
        var dto = new ChangeAdvertisementStatusDto
        {
            StatusDto = default // Draft = 0
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
