using BulletinBoard.Domain.Entities;
using BulletinBoard.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateUser()
    {
        // Arrange & Act
        var user = new User("John Doe", "john@test.com", "hashedPassword123", "+79991234567");

        // Assert
        user.DisplayName.Should().Be("John Doe");
        user.Email.Should().Be("john@test.com");
        user.PasswordHash.Should().Be("hashedPassword123");
        user.Phone.Should().Be("+79991234567");
        user.Role.Should().Be(UserRole.User);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidDisplayName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act
        var act = () => new User(invalidName!, "email@test.com", "hash", null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*имя пользователя*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act
        var act = () => new User("Name", invalidEmail!, "hash", null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*почта*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidPasswordHash_ShouldThrowArgumentException(string? invalidHash)
    {
        // Act
        var act = () => new User("Name", "email@test.com", invalidHash!, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*хеш пароля*");
    }

    [Fact]
    public void Constructor_WithNullPhone_ShouldSetPhoneToNull()
    {
        // Act
        var user = new User("Name", "email@test.com", "hash", null);

        // Assert
        user.Phone.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldTrimInputs()
    {
        // Act
        var user = new User("  Name  ", "  email@test.com  ", "hash", "  +79991234567  ");

        // Assert
        user.DisplayName.Should().Be("Name");
        user.Email.Should().Be("email@test.com");
        user.Phone.Should().Be("+79991234567");
    }

    [Fact]
    public void Constructor_WithAdminRole_ShouldSetRoleToAdmin()
    {
        // Act
        var user = new User("Admin", "admin@test.com", "hash", null, UserRole.Admin);

        // Assert
        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void UpdateUser_WithValidDisplayName_ShouldUpdateDisplayName()
    {
        // Arrange
        var user = new User("Old Name", "email@test.com", "hash");

        // Act
        user.UpdateUser(displayName: "New Name");

        // Assert
        user.DisplayName.Should().Be("New Name");
    }

    [Fact]
    public void UpdateUser_WithNullDisplayName_ShouldNotChangeDisplayName()
    {
        // Arrange
        var user = new User("Name", "email@test.com", "hash");

        // Act
        user.UpdateUser(displayName: null);

        // Assert
        user.DisplayName.Should().Be("Name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateUser_WithEmptyDisplayName_ShouldThrowArgumentException(string emptyName)
    {
        // Arrange
        var user = new User("Name", "email@test.com", "hash");

        // Act
        var act = () => user.UpdateUser(displayName: emptyName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*имя пользователя*");
    }

    [Fact]
    public void UpdateUser_WithValidEmail_ShouldUpdateEmail()
    {
        // Arrange
        var user = new User("Name", "old@test.com", "hash");

        // Act
        user.UpdateUser(email: "new@test.com");

        // Assert
        user.Email.Should().Be("new@test.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateUser_WithEmptyEmail_ShouldThrowArgumentException(string emptyEmail)
    {
        // Arrange
        var user = new User("Name", "email@test.com", "hash");

        // Act
        var act = () => user.UpdateUser(email: emptyEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*почта*");
    }

    [Fact]
    public void UpdateUser_WithEmptyPhone_ShouldSetPhoneToNull()
    {
        // Arrange
        var user = new User("Name", "email@test.com", "hash", "+79991234567");

        // Act
        user.UpdateUser(phone: "");

        // Assert
        user.Phone.Should().BeNull();
    }

    [Fact]
    public void UpdatePassword_WithValidHash_ShouldUpdatePassword()
    {
        // Arrange
        var user = new User("Name", "email@test.com", "oldHash");

        // Act
        user.UpdatePassword("newHash");

        // Assert
        user.PasswordHash.Should().Be("newHash");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdatePassword_WithInvalidHash_ShouldThrowArgumentException(string? invalidHash)
    {
        // Arrange
        var user = new User("Name", "email@test.com", "hash");

        // Act
        var act = () => user.UpdatePassword(invalidHash!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PromoteToAdmin_ShouldChangeRoleToAdmin()
    {
        // Arrange
        var user = new User("Name", "email@test.com", "hash");
        user.Role.Should().Be(UserRole.User);

        // Act
        user.PromoteToAdmin();

        // Assert
        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void DemoteToUser_ShouldChangeRoleToUser()
    {
        // Arrange
        var user = new User("Admin", "admin@test.com", "hash", null, UserRole.Admin);
        user.Role.Should().Be(UserRole.Admin);

        // Act
        user.DemoteToUser();

        // Assert
        user.Role.Should().Be(UserRole.User);
    }
}