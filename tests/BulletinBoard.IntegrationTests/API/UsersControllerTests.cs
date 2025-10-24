using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BulletinBoard.Contracts.Auth;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Contracts.Users;
using BulletinBoard.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.IntegrationTests.API;

public class UsersControllerTests : IntegrationTestBase
{
    public UsersControllerTests(BulletinBoardWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsUser()
    {
        // Arrange
        var client = CreateClient();
        var (_, userId) = await AuthenticateAsUserAsync(client);

        // Act
        var response = await client.GetAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.DisplayName.Should().NotBeNullOrEmpty();
        result.Email.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPage_ReturnsPaginatedUsers()
    {
        // Arrange
        var client = CreateClient();
        // Создаём нескольких пользователей
        await AuthenticateAsUserAsync(client);
        await AuthenticateAsUserAsync(CreateClient());
        await AuthenticateAsUserAsync(CreateClient());

        // Act
        var response = await client.GetAsync("/api/users?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterOrEqualTo(3);
        result.TotalCount.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task Update_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateClient();
        var updateDto = new UpdateUserDto
        {
            DisplayName = "New Name"
        };

        // Act - попытка обновить без токена
        var response = await client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsOwner_UpdatesSuccessfully()
    {
        // Arrange
        var client = CreateClient();
        var (token, userId) = await AuthenticateAsUserAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateDto = new UpdateUserDto
        {
            DisplayName = "Updated Name",
            Phone = "+79991234567"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{userId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("Updated Name");
        result.Phone.Should().Be("+79991234567");
    }

    [Fact]
    public async Task Update_AsOtherUser_ReturnsForbidden()
    {
        // Arrange
        var client1 = CreateClient();
        var client2 = CreateClient();
        var (token1, userId1) = await AuthenticateAsUserAsync(client1);
        var (_, userId2) = await AuthenticateAsUserAsync(client2);

        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var updateDto = new UpdateUserDto
        {
            DisplayName = "New Name"
        };

        // Act - попытка пользователя 1 обновить пользователя 2
        var response = await client1.PutAsJsonAsync($"/api/users/{userId2}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCurrentUser_WithAuth_ReturnsUserInfo()
    {
        // Arrange
        var client = CreateClient();
        var (token, userId) = await AuthenticateAsUserAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserInfoDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.DisplayName.Should().NotBeNullOrEmpty();
        result.Email.Should().NotBeNullOrEmpty();
        result.Role.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateClient();

        // Act - запрос без токена
        var response = await client.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
