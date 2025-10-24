using BulletinBoard.Domain.Base;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Domain.Base;

file class TestEntity : EntityBase { }

public class EntityBaseTests
{
    [Fact]
    public void NewEntity_ShouldHaveUniqueId()
    {
        // Arrange & Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Assert
        entity1.Id.Should().NotBe(Guid.Empty);
        entity2.Id.Should().NotBe(Guid.Empty);
        entity1.Id.Should().NotBe(entity2.Id);
    }

    [Fact]
    public void NewEntity_ShouldHaveCreatedAtNearNow()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void NewEntity_UpdatedAtShouldBeNull()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.UpdatedAt.Should().BeNull();  // ✅ ИСПРАВЛЕНО
    }
}