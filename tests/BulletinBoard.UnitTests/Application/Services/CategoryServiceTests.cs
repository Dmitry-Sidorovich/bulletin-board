using AutoMapper;
using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Contracts.Categories;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockRepository;
    private readonly Mock<ICategoryReadRepository> _mockReadRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _mockRepository = new Mock<ICategoryRepository>();
        _mockReadRepository = new Mock<ICategoryReadRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockCacheService = new Mock<ICacheService>();

        _service = new CategoryService(
            _mockRepository.Object,
            _mockReadRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockCacheService.Object);
    }

    [Fact]
    public async Task GetRootsAsync_ShouldReturnRootCategories()
    {
        // Arrange
        var expectedCategories = new List<CategoryDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Category1" },
            new() { Id = Guid.NewGuid(), Name = "Category2" }
        };
        _mockReadRepository.Setup(r => r.GetRootsAsync(default))
            .ReturnsAsync(expectedCategories);

        // Act
        var result = await _service.GetRootsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedCategories);
    }

    [Fact]
    public async Task CreateRootAsync_WithValidName_ShouldCreateCategory()
    {
        // Arrange
        var name = "New Category";
        var expectedDto = new CategoryDto { Id = Guid.NewGuid(), Name = name };
        _mockMapper.Setup(m => m.Map<CategoryDto>(It.IsAny<Category>()))
            .Returns(expectedDto);

        // Act
        var result = await _service.CreateRootAsync(name);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Category>(), default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateRootAsync_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act
        var act = async () => await _service.CreateRootAsync(invalidName!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateChildAsync_WithValidData_ShouldCreateChildCategory()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var name = "Child Category";
        var expectedDto = new CategoryDto { Id = Guid.NewGuid(), Name = name, ParentId = parentId };
        _mockMapper.Setup(m => m.Map<CategoryDto>(It.IsAny<Category>()))
            .Returns(expectedDto);

        // Act
        var result = await _service.CreateChildAsync(parentId, name);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.ParentId.Should().Be(parentId);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Category>(), default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        _mockCacheService.Verify(c => c.RemoveAsync("categories:root", default), Times.Once);
    }

    [Fact]
    public async Task CreateChildAsync_WithEmptyParentId_ShouldThrowArgumentException()
    {
        // Act
        var act = async () => await _service.CreateChildAsync(Guid.Empty, "Name");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category("Test Category");
        _mockRepository.Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync(existingCategory);

        // Act
        var result = await _service.DeleteAsync(categoryId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(categoryId, default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
        _mockCacheService.Verify(c => c.RemoveAsync("categories:root", default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotExists_ShouldReturnFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _service.DeleteAsync(categoryId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), default), Times.Never);
    }
    
    //new
    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var expectedDto = new CategoryDto { Id = categoryId, Name = "Electronics" };
        
        _mockReadRepository.Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _service.GetByIdAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(categoryId);
        result.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        
        _mockReadRepository.Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync((CategoryDto?)null);

        // Act
        var result = await _service.GetByIdAsync(categoryId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetChildrenAsync_ShouldReturnChildCategories()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var page = new PageRequest { Page = 1, PageSize = 10 };
        
        var expectedResult = new PagedResult<CategoryDto>
        {
            Items = new List<CategoryDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Smartphones", ParentId = parentId },
                new() { Id = Guid.NewGuid(), Name = "Laptops", ParentId = parentId }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        
        _mockReadRepository.Setup(r => r.GetChildrenAsync(parentId, page, default))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.GetChildrenAsync(parentId, page);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.All(c => c.ParentId == parentId).Should().BeTrue();
    }
}