using BulletinBoard.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Domain.Entities;

public class AdvertisementFileTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateAdvertisementFile()
    {
        // Arrange
        var advertisementId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var order = 1;

        // Act
        var advertisementFile = new AdvertisementFile(advertisementId, fileId, order);

        // Assert
        advertisementFile.AdvertisementId.Should().Be(advertisementId);
        advertisementFile.FileId.Should().Be(fileId);
        advertisementFile.Order.Should().Be(order);
    }

    [Fact]
    public void Constructor_WithEmptyAdvertisementId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new AdvertisementFile(Guid.Empty, Guid.NewGuid(), 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("advertisementId");
    }

    [Fact]
    public void Constructor_WithEmptyFileId_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new AdvertisementFile(Guid.NewGuid(), Guid.Empty, 1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("fileId");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithNegativeOrder_ShouldThrowArgumentException(int invalidOrder)
    {
        // Act
        var act = () => new AdvertisementFile(Guid.NewGuid(), Guid.NewGuid(), invalidOrder);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("order");
    }

    [Fact]
    public void Constructor_WithZeroOrder_ShouldSucceed()
    {
        // Act
        var advertisementFile = new AdvertisementFile(Guid.NewGuid(), Guid.NewGuid(), 0);

        // Assert
        advertisementFile.Order.Should().Be(0);
    }
}