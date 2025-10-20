using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BulletinBoard.Contracts.Files;
using BulletinBoard.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace BulletinBoard.IntegrationTests.API;

public class FilesControllerTests : IntegrationTestBase
{
    public FilesControllerTests(BulletinBoardWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task UploadFile_WithAuth_UploadsSuccessfully()
    {
        // Arrange
        var client = CreateClient();
        var (token, _) = await AuthenticateAsUserAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");

        // Act
        var response = await client.PostAsync("/api/files", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<FileUploadResultDto>();
        result.Should().NotBeNull();
        result!.FileId.Should().NotBeEmpty();
        result.Url.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UploadFile_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = CreateClient();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        content.Add(fileContent, "file", "test.jpg");

        // Act
        var response = await client.PostAsync("/api/files", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFileInfo_WhenExists_ReturnsInfo()
    {
        // Arrange
        var client = CreateClient();
        var file = new Domain.Entities.File("test.jpg", "/uploads/test.jpg", "image/jpeg", 1024);
        await DbContext.Files.AddAsync(file);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await client.GetAsync($"/api/files/{file.Id}/info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FileInfoDto>();
        result.Should().NotBeNull();
        result!.FileName.Should().Be("test.jpg");
    }

    [Fact]
    public async Task GetFileInfo_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        var client = CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/files/{Guid.NewGuid()}/info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}