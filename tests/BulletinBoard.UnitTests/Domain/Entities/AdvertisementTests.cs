using BulletinBoard.Domain.Entities;
using BulletinBoard.Domain.Enums;
using BulletinBoard.Domain.ValueObjects;
using FluentAssertions;

namespace BulletinBoard.UnitTests.Domain.Entities;

/// <summary>
/// Тесты доменной модели Advertisement.
/// </summary>
public class AdvertisementTests
{
    private readonly Contact _validContact = new("Иван Иванов", "ivan@example.com", "+79991234567");

    [Fact]
    public void Constructor_WithValidData_ShouldCreateAdvertisement()
    {
        // Arrange
        var title = "Продам iPhone";
        var description = "Отличное состояние";
        var price = 50000m;
        var categoryId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        // Act
        var ad = new Advertisement(title, description, price, categoryId, authorId, _validContact);

        // Assert
        ad.Title.Should().Be(title);
        ad.Description.Should().Be(description);
        ad.Price.Should().Be(price);
        ad.CategoryId.Should().Be(categoryId);
        ad.AuthorId.Should().Be(authorId);
        ad.Contact.Should().Be(_validContact);
        ad.Status.Should().Be(AdStatus.Draft);
        ad.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
    {
        // Arrange & Act
        var act = () => new Advertisement(
            invalidTitle!,
            "Description",
            1000m,
            Guid.NewGuid(),
            Guid.NewGuid(),
            _validContact);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*заголовок*");
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new Advertisement(
            "Title",
            "Description",
            -100m,
            Guid.NewGuid(),
            Guid.NewGuid(),
            _validContact);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*цена*");
    }

    [Fact]
    public void Constructor_WithZeroPrice_ShouldSucceed()
    {
        // Arrange & Act
        var ad = new Advertisement(
            "Title",
            "Description",
            0m,
            Guid.NewGuid(),
            Guid.NewGuid(),
            _validContact);

        // Assert
        ad.Price.Should().Be(0m);
    }

    [Fact]
    public void Constructor_WithEmptyCategoryId_ShouldThrowArgumentException()
    {
        // Arrange & Act
        var act = () => new Advertisement(
            "Title",
            "Description",
            1000m,
            Guid.Empty,
            Guid.NewGuid(),
            _validContact);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*категория*");
    }

    [Fact]
    public void Constructor_WithNullContact_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        var act = () => new Advertisement(
            "Title",
            "Description",
            1000m,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateText_WithValidData_ShouldUpdateTitleAndDescription()
    {
        // Arrange
        var ad = new Advertisement("Old Title", "Old Desc", 100m, Guid.NewGuid(), Guid.NewGuid(), _validContact);
        var newTitle = "New Title";
        var newDescription = "New Description";
        var newPrice = 200m;

        // Act
        ad.UpdateText(newTitle, newDescription, newPrice);

        // Assert
        ad.Title.Should().Be(newTitle);
        ad.Description.Should().Be(newDescription);
        ad.Price.Should().Be(newPrice);
    }

    [Fact]
    public void UpdateText_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var ad = new Advertisement("Title", null, 100m, Guid.NewGuid(), Guid.NewGuid(), _validContact);

        // Act
        var act = () => ad.UpdateText("", "Description", 200m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ChangeStatus_WithValidStatus_ShouldUpdateStatus()
    {
        // Arrange
        var ad = new Advertisement("Title", null, 100m, Guid.NewGuid(), Guid.NewGuid(), _validContact);
        ad.Status.Should().Be(AdStatus.Draft);

        // Act
        ad.ChangeStatus(AdStatus.Published);

        // Assert
        ad.Status.Should().Be(AdStatus.Published);
    }

    [Fact]
    public void ChangeCategory_WithValidCategoryId_ShouldUpdateCategory()
    {
        // Arrange
        var oldCategoryId = Guid.NewGuid();
        var ad = new Advertisement("Title", null, 100m, oldCategoryId, Guid.NewGuid(), _validContact);
        var newCategoryId = Guid.NewGuid();

        // Act
        ad.ChangeCategory(newCategoryId);

        // Assert
        ad.CategoryId.Should().Be(newCategoryId);
    }

    [Fact]
    public void ChangeCategory_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var ad = new Advertisement("Title", null, 100m, Guid.NewGuid(), Guid.NewGuid(), _validContact);

        // Act
        var act = () => ad.ChangeCategory(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateContact_WithValidContact_ShouldUpdateContact()
    {
        // Arrange
        var ad = new Advertisement("Title", null, 100m, Guid.NewGuid(), Guid.NewGuid(), _validContact);
        var newContact = new Contact("Пётр Петров", "petr@example.com", null);

        // Act
        ad.UpdateContact(newContact);

        // Assert
        ad.Contact.Should().Be(newContact);
        ad.Contact.Name.Should().Be("Пётр Петров");
    }

    [Fact]
    public void UpdateContact_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var ad = new Advertisement("Title", null, 100m, Guid.NewGuid(), Guid.NewGuid(), _validContact);

        // Act
        var act = () => ad.UpdateContact(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithTitleExactlyMaxLength_ShouldSucceed()
    {
        // Arrange
        var title = new string('A', 200); // MaxTitleLength = 200

        // Act
        var ad = new Advertisement(title, null, 0m, Guid.NewGuid(), Guid.NewGuid(), _validContact);

        // Assert
        ad.Title.Should().HaveLength(200);
    }

    [Fact]
    public void Constructor_WithTitleExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var title = new string('A', 201); // MaxTitleLength = 200

        // Act
        var act = () => new Advertisement(title, null, 0m, Guid.NewGuid(), Guid.NewGuid(), _validContact);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*не может превышать 200 символов*");
    }

    [Fact]
    public void Constructor_WithDescriptionExactlyMaxLength_ShouldSucceed()
    {
        // Arrange
        var description = new string('A', 1000); // MaxDescriptionLength = 1000

        // Act
        var ad = new Advertisement("Title", description, 0m, Guid.NewGuid(), Guid.NewGuid(), _validContact);

        // Assert
        ad.Description.Should().HaveLength(1000);
    }

    [Fact]
    public void Constructor_WithDescriptionExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var description = new string('A', 1001); // MaxDescriptionLength = 1000

        // Act
        var act = () => new Advertisement("Title", description, 0m, Guid.NewGuid(), Guid.NewGuid(), _validContact);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*не может превышать 1000 символов*");
    }

    [Fact]
    public void Constructor_WithNullDescription_ShouldSetEmptyString()
    {
        // Act
        var ad = new Advertisement("Title", null, 0m, Guid.NewGuid(), Guid.NewGuid(), _validContact);

        // Assert
        ad.Description.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldTrimTitleAndDescription()
    {
        // Act
        var ad = new Advertisement("  Title  ", "  Description  ", 0m, Guid.NewGuid(), Guid.NewGuid(), _validContact);

        // Assert
        ad.Title.Should().Be("Title");
        ad.Description.Should().Be("Description");
    }

    [Fact]
    public void UpdateText_WithTitleExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var ad = new Advertisement("Title", null, 0m, Guid.NewGuid(), Guid.NewGuid(), _validContact);
        var longTitle = new string('A', 201);

        // Act
        var act = () => ad.UpdateText(longTitle, null, 0m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*не может превышать 200 символов*");
    }

    [Fact]
    public void UpdateText_WithDescriptionExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var ad = new Advertisement("Title", null, 0m, Guid.NewGuid(), Guid.NewGuid(), _validContact);
        var longDescription = new string('A', 1001);

        // Act
        var act = () => ad.UpdateText("Title", longDescription, 0m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*не может превышать 1000 символов*");
    }

    [Fact]
    public void UpdateText_WithNullDescription_ShouldSetEmptyString()
    {
        // Arrange
        var ad = new Advertisement("Old Title", "Old Description", 100m, Guid.NewGuid(), Guid.NewGuid(), _validContact);

        // Act
        ad.UpdateText("New Title", null, 100m);

        // Assert
        ad.Description.Should().BeEmpty();
    }

    [Fact]
    public void ChangeStatus_ToSameStatus_ShouldNotThrow()
    {
        // Arrange
        var ad = new Advertisement("Title", null, 0m, Guid.NewGuid(), Guid.NewGuid(), _validContact);
        ad.ChangeStatus(AdStatus.Published);

        // Act
        var act = () => ad.ChangeStatus(AdStatus.Published);

        // Assert
        act.Should().NotThrow();
        ad.Status.Should().Be(AdStatus.Published);
    }
}