using BulletinBoard.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Domain.Entities;

public class CategoryTests
{
    [Fact]
    public void Constructor_WithValidName_ShouldCreateRootCategory()
    {
        // Arrange & Act
        var category = new Category("Электроника");

        // Assert
        category.Name.Should().Be("Электроника");
        category.ParentId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithValidNameAndParent_ShouldCreateChildCategory()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        // Act
        var category = new Category("Смартфоны", parentId);

        // Assert
        category.Name.Should().Be("Смартфоны");
        category.ParentId.Should().Be(parentId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act
        var act = () => new Category(invalidName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*название категории*");
    }

    [Fact]
    public void Constructor_WithNameExactlyMaxLength_ShouldSucceed()
    {
        // Arrange
        var name = new string('A', 100); // MaxNameLength = 100

        // Act
        var category = new Category(name);

        // Assert
        category.Name.Should().HaveLength(100);
    }

    [Fact]
    public void Constructor_WithNameExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var name = new string('A', 101); // MaxNameLength = 100

        // Act
        var act = () => new Category(name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*не может превышать 100 символов*");
    }

    [Fact]
    public void Constructor_ShouldTrimName()
    {
        // Act
        var category = new Category("  Электроника  ");

        // Assert
        category.Name.Should().Be("Электроника");
    }
}