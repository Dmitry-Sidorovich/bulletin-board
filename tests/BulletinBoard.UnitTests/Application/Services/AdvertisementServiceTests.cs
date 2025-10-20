using AutoMapper;
using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Application.Exceptions;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Domain.Enums;
using BulletinBoard.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Services;

public class AdvertisementServiceTests
{
    private readonly Mock<IAdvertisementRepository> _mockRepository;
    private readonly Mock<IAdvertisementReadRepository> _mockReadRepository;
    private readonly Mock<IAdvertisementFileRepository> _mockFileRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly AdvertisementService _service;

    public AdvertisementServiceTests()
    {
        _mockRepository = new Mock<IAdvertisementRepository>();
        _mockReadRepository = new Mock<IAdvertisementReadRepository>();
        _mockFileRepository = new Mock<IAdvertisementFileRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCacheService = new Mock<ICacheService>();

        _service = new AdvertisementService(
            _mockRepository.Object,
            _mockReadRepository.Object,
            _mockFileRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockCurrentUserService.Object,
            _mockCacheService.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedDto = new AdvertisementDto { Id = id, Title = "Test" };
        _mockReadRepository.Setup(r => r.GetByIdAsync(id, default))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        _mockReadRepository.Verify(r => r.GetByIdAsync(id, default), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockReadRepository.Setup(r => r.GetByIdAsync(id, default))
            .ReturnsAsync((AdvertisementDto?)null);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateAdvertisement()
    {
        // Arrange
        var currentUserId = Guid.NewGuid(); // <-- 1. Определяем тестовый ID
        _mockCurrentUserService.Setup(s => s.GetCurrentUserId()).Returns(currentUserId); // <-- 2. Настраиваем мок

        var createDto = new CreateAdvertisementDto
        {
            Title = "Test Ad",
            Description = "Description",
            Price = 1000m,
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "John", Email = "john@test.com", Phone = null }
        };

        Advertisement? capturedAd = null;
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Advertisement>(), default))
            .Callback<Advertisement, CancellationToken>((ad, ct) => capturedAd = ad);

        _mockMapper.Setup(m => m.Map<AdvertisementDto>(It.IsAny<Advertisement>()))
            .Returns(
                (Advertisement ad) => new AdvertisementDto { Id = ad.Id, Title = ad.Title, AuthorId = ad.AuthorId });

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Advertisement>(), default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);

        capturedAd.Should().NotBeNull();
        capturedAd!.AuthorId.Should().Be(currentUserId); // <-- 3. Теперь эта проверка тоже будет работать
    }

    [Fact]
    public async Task CreateAsync_WithNullDto_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _service.CreateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
    {
        // Arrange
        var createDto = new CreateAdvertisementDto
        {
            Title = invalidTitle!,
            Price = 1000m,
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var act = async () => await _service.CreateAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("dto");
    }

    [Fact]
    public async Task UpdateAsync_AsOwner_ShouldUpdateAdvertisement()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var existingAd = new Advertisement("Old Title", null, 100m, Guid.NewGuid(), authorId,
            new Contact("John", "john@test.com"));

        var updateDto = new UpdateAdvertisementDto
        {
            Title = "New Title",
            Description = "New Description",
            Price = 200m,
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "Jane", Email = "jane@test.com" }
        };

        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync(existingAd);
        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(authorId))
            .Returns(true);
        _mockMapper.Setup(m => m.Map<AdvertisementDto>(It.IsAny<Advertisement>()))
            .Returns(new AdvertisementDto { Id = adId, Title = updateDto.Title });

        // Act
        var result = await _service.UpdateAsync(adId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(updateDto.Title);
        _mockRepository.Verify(r => r.UpdateAsync(existingAd, default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        _mockCacheService.Verify(c => c.RemoveAsync($"ad:{adId}", default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_AsNonOwner_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var existingAd = new Advertisement("Title", null, 100m, Guid.NewGuid(), authorId,
            new Contact("John", "john@test.com"));

        var updateDto = new UpdateAdvertisementDto
        {
            Title = "New Title",
            Price = 200m,
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "Jane", Email = "jane@test.com" }
        };

        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync(existingAd);
        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(authorId))
            .Returns(false);

        // Act
        var act = async () => await _service.UpdateAsync(adId, updateDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var updateDto = new UpdateAdvertisementDto
        {
            Title = "New Title",
            Price = 200m,
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "Jane", Email = "jane@test.com" }
        };

        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync((Advertisement?)null);

        // Act
        var act = async () => await _service.UpdateAsync(adId, updateDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var existingAd = new Advertisement("Title", null, 100m, Guid.NewGuid(), Guid.NewGuid(),
            new Contact("John", "john@test.com"));

        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync(existingAd);

        // Act
        var result = await _service.DeleteAsync(adId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(adId, default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        _mockCacheService.Verify(c => c.RemoveAsync($"ad:{adId}", default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotExists_ShouldReturnFalse()
    {
        // Arrange
        var adId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync((Advertisement?)null);

        // Act
        var result = await _service.DeleteAsync(adId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    [Fact]
    public async Task ChangeStatusAsync_AsOwner_ShouldChangeStatus()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var existingAd = new Advertisement("Title", null, 100m, Guid.NewGuid(), authorId,
            new Contact("John", "john@test.com"));

        var statusDto = new ChangeAdvertisementStatusDto { StatusDto = AdStatusDto.Published };

        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync(existingAd);
        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(authorId))
            .Returns(true);

        // Act
        var result = await _service.ChangeStatusAsync(adId, statusDto);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(existingAd, default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AttachFileAsync_AsOwner_ShouldAttachFile()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var existingAd = new Advertisement("Title", null, 100m, Guid.NewGuid(), authorId,
            new Contact("John", "john@test.com"));

        _mockFileRepository.Setup(r => r.AdvertisementExistsAsync(adId, default))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync(existingAd);
        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(authorId))
            .Returns(true);
        _mockFileRepository.Setup(r => r.FileExistsAsync(fileId, default))
            .ReturnsAsync(true);
        _mockFileRepository.Setup(r => r.IsFileAttachedAsync(adId, fileId, default))
            .ReturnsAsync(false);
        _mockFileRepository.Setup(r => r.GetMaxOrderAsync(adId, default))
            .ReturnsAsync(0);

        // Act
        var result = await _service.AttachFileAsync(adId, fileId);

        // Assert
        result.Should().BeTrue();
        _mockFileRepository.Verify(r => r.AddAsync(It.IsAny<AdvertisementFile>(), default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnPagedResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var page = new PageRequest { Page = 1, PageSize = 10 };
        var expectedResult = new PagedResult<AdvertisementDto>
        {
            Items = new List<AdvertisementDto>
            {
                new() { Id = Guid.NewGuid(), Title = "Ad 1" },
                new() { Id = Guid.NewGuid(), Title = "Ad 2" }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        _mockReadRepository.Setup(r => r.GetByCategoryAsync(categoryId, page, default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.GetByCategoryAsync(categoryId, page);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetByAuthorAsync_ShouldReturnPagedResult()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var page = new PageRequest { Page = 1, PageSize = 10 };
        var expectedResult = new PagedResult<AdvertisementDto>
        {
            Items = new List<AdvertisementDto>
            {
                new() { Id = Guid.NewGuid(), Title = "My Ad", AuthorId = authorId }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        _mockReadRepository.Setup(r => r.GetByAuthorAsync(authorId, page, default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.GetByAuthorAsync(authorId, page);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().AuthorId.Should().Be(authorId);
    }

    [Fact]
    public async Task ChangeStatusAsync_ToSameStatus_ShouldReturnTrue()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var existingAd = new Advertisement("Title", null, 100m, Guid.NewGuid(), authorId,
            new Contact("John", "john@test.com"));
        existingAd.ChangeStatus(AdStatus.Published);

        var statusDto = new ChangeAdvertisementStatusDto { StatusDto = AdStatusDto.Published };

        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync(existingAd);
        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(authorId))
            .Returns(true);

        // Act
        var result = await _service.ChangeStatusAsync(adId, statusDto);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Advertisement>(), default), Times.Never);
    }

    [Fact]
    public async Task DetachFileAsync_AsOwner_ShouldDetachFile()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var existingAd = new Advertisement("Title", null, 100m, Guid.NewGuid(), authorId,
            new Contact("John", "john@test.com"));

        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync(existingAd);
        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(authorId))
            .Returns(true);
        _mockFileRepository.Setup(r => r.DeleteAsync(adId, fileId, default))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DetachFileAsync(adId, fileId);

        // Assert
        result.Should().BeTrue();
        _mockCacheService.Verify(c => c.RemoveAsync($"ad:{adId}", default), Times.Once);
    }

    [Fact]
    public async Task DetachFileAsync_WhenFileNotAttached_ShouldReturnFalse()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var existingAd = new Advertisement("Title", null, 100m, Guid.NewGuid(), authorId,
            new Contact("John", "john@test.com"));

        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync(existingAd);
        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(authorId))
            .Returns(true);
        _mockFileRepository.Setup(r => r.DeleteAsync(adId, fileId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DetachFileAsync(adId, fileId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AttachFileAsync_WhenAdvertisementNotExists_ShouldReturnFalse()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        _mockFileRepository.Setup(r => r.AdvertisementExistsAsync(adId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.AttachFileAsync(adId, fileId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AttachFileAsync_WhenFileNotExists_ShouldReturnFalse()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var existingAd = new Advertisement("Title", null, 100m, Guid.NewGuid(), authorId,
            new Contact("John", "john@test.com"));

        _mockFileRepository.Setup(r => r.AdvertisementExistsAsync(adId, default))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync(existingAd);
        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(authorId))
            .Returns(true);
        _mockFileRepository.Setup(r => r.FileExistsAsync(fileId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.AttachFileAsync(adId, fileId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AttachFileAsync_WhenFileAlreadyAttached_ShouldReturnTrue()
    {
        // Arrange
        var adId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var existingAd = new Advertisement("Title", null, 100m, Guid.NewGuid(), authorId,
            new Contact("John", "john@test.com"));

        _mockFileRepository.Setup(r => r.AdvertisementExistsAsync(adId, default))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.GetByIdAsync(adId, default))
            .ReturnsAsync(existingAd);
        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(authorId))
            .Returns(true);
        _mockFileRepository.Setup(r => r.FileExistsAsync(fileId, default))
            .ReturnsAsync(true);
        _mockFileRepository.Setup(r => r.IsFileAttachedAsync(adId, fileId, default))
            .ReturnsAsync(true);

        // Act
        var result = await _service.AttachFileAsync(adId, fileId);

        // Assert
        result.Should().BeTrue();
        _mockFileRepository.Verify(r => r.AddAsync(It.IsAny<AdvertisementFile>(), default), Times.Never);
    }

    // new
    [Fact]
    public async Task SearchAsync_WithEmptyFilter_ShouldReturnPagedResult()
    {
        // Arrange
        var filter = new AdvertisementFilterDto { PageNumber = 1, PageSize = 10 };
        var expectedResult = new PagedResult<AdvertisementDto>
        {
            Items = new List<AdvertisementDto>
            {
                new() { Id = Guid.NewGuid(), Title = "Ad 1" },
                new() { Id = Guid.NewGuid(), Title = "Ad 2" }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        _mockReadRepository.Setup(r => r.SearchAsync(
                It.IsAny<AdvertisementFilterDto>(),
                default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.SearchAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        _mockReadRepository.Verify(r => r.SearchAsync(
                It.Is<AdvertisementFilterDto>(f =>
                    f.PageNumber == 1 && f.PageSize == 10),
                default),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithFilters_ShouldPassFiltersToRepository()
    {
        // Arrange
        var filter = new AdvertisementFilterDto
        {
            SearchQuery = "iPhone",
            MinPrice = 10000m,
            MaxPrice = 50000m,
            CategoryId = Guid.NewGuid(),
            Status = AdStatusDto.Published,
            PageNumber = 1,
            PageSize = 10
        };

        var expectedResult = new PagedResult<AdvertisementDto>
        {
            Items = new List<AdvertisementDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        _mockReadRepository.Setup(r => r.SearchAsync(filter, default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.SearchAsync(filter);

        // Assert
        result.Should().NotBeNull();
        _mockReadRepository.Verify(r => r.SearchAsync(
                It.Is<AdvertisementFilterDto>(f =>
                    f.SearchQuery == "iPhone" &&
                    f.MinPrice == 10000m &&
                    f.MaxPrice == 50000m &&
                    f.CategoryId == filter.CategoryId &&
                    f.Status == AdStatusDto.Published),
                default),
            Times.Once);
    }
}