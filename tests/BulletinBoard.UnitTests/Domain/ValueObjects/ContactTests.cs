using BulletinBoard.Domain.ValueObjects;
using FluentAssertions;

namespace BulletinBoard.UnitTests.Domain.ValueObjects;

/// <summary>
/// Тесты ValueObject Contact.
/// </summary>
public class ContactTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateContact()
    {
        // Arrange & Act
        var contact = new Contact("Иван Иванов", "ivan@test.com", "+79991234567");

        // Assert
        contact.Name.Should().Be("Иван Иванов");
        contact.Email.Should().Be("ivan@test.com");
        contact.Phone.Should().Be("+79991234567");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Act
        var act = () => new Contact(invalidName!, "email@test.com", null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act
        var act = () => new Contact("Name", invalidEmail!, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("email");
    }

    [Fact]
    public void Constructor_WithNullPhone_ShouldSucceed()
    {
        // Act
        var contact = new Contact("Name", "email@test.com", null);

        // Assert
        contact.Phone.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyPhone_ShouldSetPhoneToNull(string emptyPhone)
    {
        // Act
        var contact = new Contact("Name", "email@test.com", emptyPhone);

        // Assert
        contact.Phone.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldTrimNameAndEmail()
    {
        // Act
        var contact = new Contact("  Name  ", "  email@test.com  ", "  +79991234567  ");

        // Assert
        contact.Name.Should().Be("Name");
        contact.Email.Should().Be("email@test.com");
        contact.Phone.Should().Be("+79991234567");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var contact1 = new Contact("Иван", "ivan@test.com", "+79991234567");
        var contact2 = new Contact("Иван", "ivan@test.com", "+79991234567");

        // Act & Assert
        contact1.Should().Be(contact2);
        (contact1 == contact2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var contact1 = new Contact("Иван", "ivan@test.com", "+79991234567");
        var contact2 = new Contact("Пётр", "petr@test.com", "+79991234567");

        // Act & Assert
        contact1.Should().NotBe(contact2);
        (contact1 != contact2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHash()
    {
        // Arrange
        var contact1 = new Contact("Иван", "ivan@test.com", "+79991234567");
        var contact2 = new Contact("Иван", "ivan@test.com", "+79991234567");

        // Act & Assert
        contact1.GetHashCode().Should().Be(contact2.GetHashCode());
    }
}