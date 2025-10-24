using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Domain.Enums;
using BulletinBoard.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.UnitTests.Application.Contexts.Advertisements;

public class AdvertisementPredicateBuilderTests
{
    private readonly List<Advertisement> _testData;

    public AdvertisementPredicateBuilderTests()
    {
        var contact = new Contact("Test User", "test@example.com", "+79991234567");
        var categoryId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        _testData = new List<Advertisement>
        {
            new("iPhone 13 Pro", "Отличное состояние", 50000m, categoryId, authorId, contact, AdStatus.Published),
            new("Samsung Galaxy", "Новый", 40000m, categoryId, authorId, contact, AdStatus.Published),
            new("Xiaomi Redmi", "Б/У", 15000m, categoryId, authorId, contact, AdStatus.Archived)
        };
    }

    [Fact]
    public void Build_WithEmptyFilter_ShouldReturnAllAdvertisements()
    {
        // Arrange
        var filter = new AdvertisementFilterDto();

        // Act
        var predicate = AdvertisementPredicateBuilder.Build(filter);
        var compiledPredicate = predicate.Compile();
        var result = _testData.Where(compiledPredicate).ToList();

        // Assert
        result.Should().HaveCount(3); 
    }

    [Fact]
    public void Build_WithSearchQuery_ShouldFilterByTitleAndDescription()
    {
        // Arrange
        var filter = new AdvertisementFilterDto { SearchQuery = "iPhone" };

        // Act
        var predicate = AdvertisementPredicateBuilder.Build(filter);
        var compiledPredicate = predicate.Compile();
        var result = _testData.Where(compiledPredicate).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Contain("iPhone");
    }

    [Fact]
    public void Build_WithSearchQuery_ShouldBeCaseInsensitive()
    {
        // Arrange
        var filter = new AdvertisementFilterDto { SearchQuery = "iphone" };

        // Act
        var predicate = AdvertisementPredicateBuilder.Build(filter);
        var compiledPredicate = predicate.Compile();
        var result = _testData.Where(compiledPredicate).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Contain("iPhone");
    }

    [Fact]
    public void Build_WithMinPrice_ShouldFilterCorrectly()
    {
        // Arrange
        var filter = new AdvertisementFilterDto { MinPrice = 30000m };

        // Act
        var predicate = AdvertisementPredicateBuilder.Build(filter);
        var compiledPredicate = predicate.Compile();
        var result = _testData.Where(compiledPredicate).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(ad => ad.Price >= 30000m).Should().BeTrue();
    }

    [Fact]
    public void Build_WithMaxPrice_ShouldFilterCorrectly()
    {
        // Arrange
        var filter = new AdvertisementFilterDto { MaxPrice = 20000m };

        // Act
        var predicate = AdvertisementPredicateBuilder.Build(filter);
        var compiledPredicate = predicate.Compile();
        var result = _testData.Where(compiledPredicate).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Contain("Xiaomi");
    }

    [Fact]
    public void Build_WithPriceRange_ShouldFilterCorrectly()
    {
        // Arrange
        var filter = new AdvertisementFilterDto 
        { 
            MinPrice = 20000m,
            MaxPrice = 50000m
        };

        // Act
        var predicate = AdvertisementPredicateBuilder.Build(filter);
        var compiledPredicate = predicate.Compile();
        var result = _testData.Where(compiledPredicate).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void Build_WithStatus_ShouldFilterCorrectly()
    {
        // Arrange
        var filter = new AdvertisementFilterDto { Status = AdStatusDto.Published };
        // Act
        var result = _testData.Where(AdvertisementPredicateBuilder.Build(filter).Compile()).ToList();
        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void Build_WithMultipleFilters_ShouldCombineCorrectly()
    {
        // Arrange
        var filter = new AdvertisementFilterDto
        {
            SearchQuery = "Galaxy",
            MinPrice = 30000m,
            Status = AdStatusDto.Published
        };

        // Act
        var predicate = AdvertisementPredicateBuilder.Build(filter);
        var compiledPredicate = predicate.Compile();
        var result = _testData.Where(compiledPredicate).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Contain("Galaxy");
    }
    
    [Fact]
    public void Build_WithCategoryId_ShouldFilterCorrectly()
    {
        // Arrange
        var targetCategoryId = Guid.NewGuid();
        var contact = new Contact("Test User", "test@example.com", "+79991234567");
    
        var testData = new List<Advertisement>
        {
            new("Ad 1", null, 1000m, targetCategoryId, Guid.NewGuid(), contact, AdStatus.Published),
            new("Ad 2", null, 2000m, Guid.NewGuid(), Guid.NewGuid(), contact, AdStatus.Published),
            new("Ad 3", null, 3000m, targetCategoryId, Guid.NewGuid(), contact, AdStatus.Published)
        };
    
        var filter = new AdvertisementFilterDto { CategoryId = targetCategoryId };

        // Act
        var predicate = AdvertisementPredicateBuilder.Build(filter);
        var compiledPredicate = predicate.Compile();
        var result = testData.Where(compiledPredicate).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.All(ad => ad.CategoryId == targetCategoryId).Should().BeTrue();
    }
}