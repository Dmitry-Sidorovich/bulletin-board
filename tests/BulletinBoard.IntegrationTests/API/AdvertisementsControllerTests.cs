using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Domain.Enums;
using BulletinBoard.Domain.ValueObjects;
using BulletinBoard.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.IntegrationTests.API;

public class AdvertisementsControllerTests : IntegrationTestBase
{
    public AdvertisementsControllerTests(BulletinBoardWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAdvertisements_ReturnsPagedResult()
    {
        // Arrange
        var client = CreateClient();
        var (_, userId) = await AuthenticateAsUserAsync(client);
        await SeedTestDataAsync(userId);

        // Act
        var response = await client.GetAsync("/api/advertisements?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AdvertisementDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAdvertisementById_WhenExists_ReturnsAdvertisement()
    {
        // Arrange
        var client = CreateClient();
        var (_, userId) = await AuthenticateAsUserAsync(client);
        var ad = await SeedTestDataAsync(userId);

        // Act
        var response = await client.GetAsync($"/api/advertisements/{ad.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AdvertisementDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(ad.Id);
    }

    [Fact]
    public async Task GetAdvertisementById_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response = await client.GetAsync($"/api/advertisements/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAdvertisement_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateClient();
        var createDto = new CreateAdvertisementDto
        {
            Title = "Test Ad",
            Price = 1000m,
            CategoryId = Guid.NewGuid(),
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/advertisements", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAdvertisement_WithAuth_CreatesSuccessfully()
    {
        // Arrange
        var client = CreateClient();
        var (token, userId) = await AuthenticateAsUserAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var category = await SeedCategoryAsync();

        var createDto = new CreateAdvertisementDto
        {
            Title = "Test Ad",
            Price = 1000m,
            CategoryId = category.Id,
            Contact = new ContactDto { Name = "John", Email = "john@test.com" }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/advertisements", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<AdvertisementDto>();
        result!.AuthorId.Should().Be(userId);
    }

    [Fact]
    public async Task UpdateAdvertisement_AsOwner_UpdatesSuccessfully()
    {
        // Arrange
        var client = CreateClient();
        var (token, userId) = await AuthenticateAsUserAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var ad = await SeedTestDataAsync(userId);
        var category = await SeedCategoryAsync();

        var updateDto = new UpdateAdvertisementDto
        {
            Title = "Updated Title", Price = 2000m, CategoryId = category.Id,
            Contact = new ContactDto { Name = "Jane", Email = "jane@test.com" }
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/advertisements/{ad.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AdvertisementDto>();
        result!.Title.Should().Be(updateDto.Title);
    }

    [Fact]
    public async Task UpdateAdvertisement_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        var ownerClient = CreateClient();
        var (_, ownerId) = await AuthenticateAsUserAsync(ownerClient);
        var ad = await SeedTestDataAsync(ownerId);

        var attackerClient = CreateClient();
        var (attackerToken, _) = await AuthenticateAsUserAsync(attackerClient);
        attackerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", attackerToken);

        var category = await SeedCategoryAsync();
        var updateDto = new UpdateAdvertisementDto
        {
            Title = "Hacked Title", Price = 1m, CategoryId = category.Id,
            Contact = new ContactDto { Name = "Hacker", Email = "hacker@test.com" }
        };

        // Act
        var response = await attackerClient.PutAsJsonAsync($"/api/advertisements/{ad.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAdvertisement_AsAdmin_DeletesSuccessfully()
    {
        // Arrange
        var userClient = CreateClient();
        var (_, userId) = await AuthenticateAsUserAsync(userClient);
        var ad = await SeedTestDataAsync(userId);

        var adminClient = CreateClient();
        var (adminToken, _) = await AuthenticateAsAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await adminClient.DeleteAsync($"/api/advertisements/{ad.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangeAdvertisementStatus_AsOwner_UpdatesSuccessfully()
    {
        // Arrange
        var client = CreateClient();
        var (token, userId) = await AuthenticateAsUserAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var ad = await SeedTestDataAsync(userId, "Test Advertisement", AdStatus.Draft);

        var statusDto = new ChangeAdvertisementStatusDto { StatusDto = AdStatusDto.Published };

        // Act
        var response = await client.PatchAsJsonAsync(
            $"/api/advertisements/{ad.Id}/statusDto",
            statusDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify status changed
        var getResponse = await client.GetAsync($"/api/advertisements/{ad.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<AdvertisementDto>();
        result!.Status.Should().Be(AdStatusDto.Published);
    }

    [Fact]
    public async Task AttachFile_AsOwner_AttachesSuccessfully()
    {
        // Arrange
        var client = CreateClient();
        var (token, userId) = await AuthenticateAsUserAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var ad = await SeedTestDataAsync(userId);

        // Создаем файл в БД
        var file = new Domain.Entities.File("test.jpg", "/uploads/test.jpg", "image/jpeg", 1024);
        await DbContext.Files.AddAsync(file);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await client.PostAsync(
            $"/api/advertisements/{ad.Id}/attach-file?fileId={file.Id}", // <-- Правильный URL
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetAdvertisements_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var client = CreateClient();
        var (token, userId) = await AuthenticateAsUserAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        // Создаем 5 объявлений
        for (int i = 0; i < 5; i++)
        {
            await SeedTestDataAsync(userId, $"Ad {i + 1}"); 
        }

        // Act - запрашиваем вторую страницу по 2 элемента
        var response = await client.GetAsync("/api/advertisements?pageNumber=2&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AdvertisementDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetAdvertisements_WithExcessivePageSize_ReturnsLimitedResults()
    {
        // Arrange
        var client = CreateClient();
        var (token, userId) = await AuthenticateAsUserAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Создаём 3 объявления
        for (int i = 0; i < 3; i++)
        {
            await SeedTestDataAsync(userId, $"Ad {i + 1}");
        }

        // Act - запрашиваем с pageSize=10000
        var response = await client.GetAsync("/api/advertisements?pageNumber=1&pageSize=10000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AdvertisementDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3); // Возвращаются только 3 объявления
    }

    [Fact]
    public async Task GetAdvertisements_BeyondTotalPages_ReturnsEmptyList()
    {
        // Arrange
        var client = CreateClient();
        var (token, userId) = await AuthenticateAsUserAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Создаём 2 объявления
        await SeedTestDataAsync(userId, "Ad 1");
        await SeedTestDataAsync(userId, "Ad 2");

        // Act - запрашиваем страницу 10 (которой не существует)
        var response = await client.GetAsync("/api/advertisements?pageNumber=10&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AdvertisementDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task SearchAdvertisements_WithPriceFilter_ReturnsFilteredResults()
    {
        // Arrange
        var client = CreateClient();
        var (_, userId) = await AuthenticateAsUserAsync(client);

        await SeedTestDataAsync(userId); // Price = 1000
        var category = await SeedCategoryAsync();
        var contact = new Contact("User", "user@test.com", null);

        var expensiveAd = new Advertisement("Expensive", null, 5000m, category.Id, userId, contact);
        await DbContext.Advertisements.AddAsync(expensiveAd);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/api/advertisements?minPrice=2000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<AdvertisementDto>>();
        result!.Items.Should().HaveCount(1);
        result.Items.First().Price.Should().Be(5000m);
    }
    private async Task<Advertisement> SeedTestDataAsync(
        Guid authorId,
        string title = "Test Advertisement",
        AdStatus status = AdStatus.Published)
    {
        var category = await SeedCategoryAsync();
        var contact = new Contact("Test User", "test@example.com", null);
        var ad = new Advertisement(title, "Desc", 1000m, category.Id, authorId, contact, status);
        DbContext.Advertisements.Add(ad);
        await DbContext.SaveChangesAsync();
        return ad;
    }

    private async Task<Category> SeedCategoryAsync()
    {
        var category = new Category("Test Category");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();
        return category;
    }
}