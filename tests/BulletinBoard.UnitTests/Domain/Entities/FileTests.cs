using FluentAssertions;
using DomainFile = BulletinBoard.Domain.Entities.File;

namespace BulletinBoard.UnitTests.Domain.Entities;

public class FileTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateFile()
    {
        // Arrange
        var fileName = "test.jpg";
        var filePath = "/uploads/2025/test.jpg";
        var contentType = "image/jpeg";
        var fileSize = 1024L;

        // Act
        var file = new DomainFile(fileName, filePath, contentType, fileSize);

        // Assert
        file.FileName.Should().Be(fileName);
        file.FilePath.Should().Be(filePath);
        file.ContentType.Should().Be(contentType);
        file.FileSize.Should().Be(fileSize);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidFileName_ShouldThrowArgumentException(string? invalidFileName)
    {
        // Act
        var act = () => new DomainFile(invalidFileName!, "/path", "image/jpeg", 1024);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("fileName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidFilePath_ShouldThrowArgumentException(string? invalidFilePath)
    {
        // Act
        var act = () => new DomainFile("test.jpg", invalidFilePath!, "image/jpeg", 1024);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("filePath");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidContentType_ShouldThrowArgumentException(string? invalidContentType)
    {
        // Act
        var act = () => new DomainFile("test.jpg", "/path", invalidContentType!, 1024);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("contentType");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void Constructor_WithInvalidFileSize_ShouldThrowArgumentException(long invalidSize)
    {
        // Act
        var act = () => new DomainFile("test.jpg", "/path", "image/jpeg", invalidSize);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("fileSize");
    }

    [Fact]
    public void Constructor_ShouldTrimStrings()
    {
        // Act
        var file = new DomainFile("  test.jpg  ", "  /path  ", "  image/jpeg  ", 1024);

        // Assert
        file.FileName.Should().Be("test.jpg");
        file.FilePath.Should().Be("/path");
        file.ContentType.Should().Be("image/jpeg");
    }
}