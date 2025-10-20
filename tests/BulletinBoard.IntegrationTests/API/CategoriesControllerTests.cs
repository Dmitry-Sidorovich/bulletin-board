using System.Net;
using System.Net.Http.Json;
using BulletinBoard.Contracts.Categories;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Domain.Entities;
using BulletinBoard.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.IntegrationTests.API;

public class CategoriesControllerTests : IntegrationTestBase
{
    public CategoriesControllerTests(BulletinBoardWebApplicationFactory factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task GetRootCategories_ReturnsCategories()
    {
        // Arrange
        var client = CreateClient();
        // Добавляем тестовые данные, чтобы было что возвращать
        await DbContext.Categories.AddAsync(new Category("Test Root 1"));
        await DbContext.Categories.AddAsync(new Category("Test Root 2"));
        await DbContext.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/api/categories/roots");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateCategory_WithAuth_CreatesSuccessfully()
    {
        // Arrange
        var client = CreateClient();
        var (token, _) = await AuthenticateAsAdminAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/categories/root", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CategoryDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task CreateCategory_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateClient();
        var createDto = new CreateCategoryDto
        {
            Name = "Electronics"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/categories/root", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateChildCategory_WithAuth_CreatesSuccessfully()
    {
        // Arrange
        var client = CreateClient();
        var (token, _) = await AuthenticateAsAdminAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Создаем родительскую категорию
        var parent = new Category("Electronics");
        await DbContext.Categories.AddAsync(parent);
        await DbContext.SaveChangesAsync();
        
        var childDto = new CreateCategoryDto { Name = "Smartphones" };

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/categories/{parent.Id}/child",
            childDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CategoryDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Smartphones");
        result.ParentId.Should().Be(parent.Id);
    }

    [Fact]
    public async Task GetChildren_ReturnsChildCategories()
    {
        // Arrange
        var client = CreateClient();
        var parent = new Category("Electronics");
        await DbContext.Categories.AddAsync(parent);
        await DbContext.Categories.AddAsync(new Category("Smartphones", parent.Id));
        await DbContext.Categories.AddAsync(new Category("Laptops", parent.Id));
        await DbContext.SaveChangesAsync();

        // Act
        var response = await client.GetAsync(
            $"/api/categories/{parent.Id}/children?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<CategoryDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteCategory_AsAdmin_DeletesSuccessfully()
    {
        // Arrange
        var client = CreateClient();
        var (token, _) = await AuthenticateAsAdminAsync(client);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var category = new Category("ToDelete");
        await DbContext.Categories.AddAsync(category);
        await DbContext.SaveChangesAsync();
        
        // Act
        var response = await client.DeleteAsync($"/api/categories/{category.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}