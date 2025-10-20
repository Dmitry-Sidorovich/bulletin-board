using AutoMapper;
using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Application.Exceptions;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Contracts.Users;
using BulletinBoard.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IUserReadRepository> _mockReadRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockReadRepository = new Mock<IUserReadRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        _service = new UserService(
            _mockRepository.Object,
            _mockReadRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedDto = new UserDto { Id = userId, DisplayName = "John Doe" };
        _mockReadRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
    }

    [Fact]
    public async Task UpdateAsync_AsOwner_ShouldUpdateUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User("Old Name", "old@test.com", "hash");
        var updateDto = new UpdateUserDto
        {
            DisplayName = "New Name",
            Email = "new@test.com",
            Phone = "+79991234567"
        };

        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(userId))
            .Returns(true);
        _mockRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync(existingUser);
        _mockMapper.Setup(m => m.Map<UserDto>(It.IsAny<User>()))
            .Returns(new UserDto { Id = userId, DisplayName = updateDto.DisplayName });

        // Act
        var result = await _service.UpdateAsync(userId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be(updateDto.DisplayName);
        _mockRepository.Verify(r => r.UpdateAsync(existingUser, default), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_AsNonOwner_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto { DisplayName = "New Name" };
        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(userId))
            .Returns(false);

        // Act
        var act = async () => await _service.UpdateAsync(userId, updateDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDto { DisplayName = "New Name" };
        _mockCurrentUserService.Setup(s => s.IsOwnerOrAdmin(userId))
            .Returns(true);
        _mockRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _service.UpdateAsync(userId, updateDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockReadRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _service.GetByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }
}