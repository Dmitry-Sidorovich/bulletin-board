using AutoMapper;
using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Files.Repositories;
using BulletinBoard.Application.Contexts.Files.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using DomainFile = BulletinBoard.Domain.Entities.File;

namespace BulletinBoard.UnitTests.Application.Services;

public class FileServiceTests
{
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<FileService>> _mockLogger;
    private readonly FileService _service;

    public FileServiceTests()
    {
        _mockFileRepository = new Mock<IFileRepository>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<FileService>>();

        _service = new FileService(
            _mockFileRepository.Object,
            _mockFileStorageService.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task UploadAsync_WithValidFile_ShouldUploadSuccessfully()
    {
        // Arrange
        var stream = new MemoryStream();
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var fileSize = 1024L;
        var savedPath = "/uploads/2025/test.jpg";

        _mockFileStorageService.Setup(s => s.SaveFileAsync(stream, fileName, default))
            .ReturnsAsync(savedPath);
        _mockFileStorageService.Setup(s => s.GetFileUrl(savedPath))
            .Returns($"https://example.com{savedPath}");

        // Act
        var result = await _service.UploadAsync(stream, fileName, contentType, fileSize);

        // Assert
        result.Should().NotBeNull();
        result.Url.Should().Contain(savedPath);
        _mockFileRepository.Verify(r => r.AddAsync(It.IsAny<DomainFile>(), default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetInfoAsync_WhenFileExists_ShouldReturnFileInfo()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var file = new DomainFile("test.jpg", "/uploads/test.jpg", "image/jpeg", 1024);

        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId, default))
            .ReturnsAsync(file);
        _mockFileStorageService.Setup(s => s.GetFileUrl(file.FilePath))
            .Returns($"https://example.com{file.FilePath}");

        // Act
        var result = await _service.GetInfoAsync(fileId);

        // Assert
        result.Should().NotBeNull();
        result!.FileName.Should().Be("test.jpg");
        result.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task GetInfoAsync_WhenFileNotExists_ShouldReturnNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId, default))
            .ReturnsAsync((DomainFile?)null);

        // Act
        var result = await _service.GetInfoAsync(fileId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DownloadAsync_WhenFileExists_ShouldReturnStream()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var file = new DomainFile("test.jpg", "/uploads/test.jpg", "image/jpeg", 1024);
        var expectedStream = new MemoryStream();

        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId, default))
            .ReturnsAsync(file);
        _mockFileStorageService.Setup(s => s.ReadFileAsync(file.FilePath, default))
            .ReturnsAsync(expectedStream);

        // Act
        var result = await _service.DownloadAsync(fileId);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Stream.Should().BeSameAs(expectedStream);
        result.Value.FileName.Should().Be("test.jpg");
        result.Value.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task DownloadAsync_WhenFileNotExists_ShouldReturnNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId, default))
            .ReturnsAsync((DomainFile?)null);

        // Act
        var result = await _service.DownloadAsync(fileId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DownloadAsync_WhenStorageReturnsNull_ShouldReturnNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var file = new DomainFile("test.jpg", "/uploads/test.jpg", "image/jpeg", 1024);

        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId, default))
            .ReturnsAsync(file);
        _mockFileStorageService.Setup(s => s.ReadFileAsync(file.FilePath, default))
            .ReturnsAsync((Stream?)null);

        // Act
        var result = await _service.DownloadAsync(fileId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenFileExists_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var file = new DomainFile("test.jpg", "/uploads/test.jpg", "image/jpeg", 1024);

        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId, default))
            .ReturnsAsync(file);

        // Act
        var result = await _service.DeleteAsync(fileId);

        // Assert
        result.Should().BeTrue();
        _mockFileStorageService.Verify(s => s.DeleteFileAsync(file.FilePath, default), Times.Once);
        _mockFileRepository.Verify(r => r.DeleteAsync(fileId, default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenFileNotExists_ShouldReturnFalse()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        _mockFileRepository.Setup(r => r.GetByIdAsync(fileId, default))
            .ReturnsAsync((DomainFile?)null);

        // Act
        var result = await _service.DeleteAsync(fileId);

        // Assert
        result.Should().BeFalse();
        _mockFileStorageService.Verify(s => s.DeleteFileAsync(It.IsAny<string>(), default), Times.Never);
        _mockFileRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task UploadAsync_WithNullStream_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _service.UploadAsync(null!, "test.jpg", "image/jpeg", 1024);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UploadAsync_WithEmptyStream_ShouldThrowArgumentException()
    {
        // Arrange
        var stream = new MemoryStream();
        var fileSize = 0L;

        // Act & Assert
        var act = async () => await _service.UploadAsync(stream, "test.jpg", "image/jpeg", fileSize);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("fileSize");
    }

    /// <summary>
    /// Проверяет защиту от DoS - загрузка файла размером больше максимально допустимого должна завершиться ошибкой.
    /// </summary>
    [Fact]
    public async Task UploadAsync_WithHugeFile_ShouldThrowArgumentException()
    {
        // Arrange
        var stream = new MemoryStream();
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var fileSize = 100_000_000_000L; // 100 GB

        // Act
        var act = async () => await _service.UploadAsync(stream, fileName, contentType, fileSize);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("fileSize");
    }
}