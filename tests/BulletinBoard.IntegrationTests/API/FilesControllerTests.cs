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
    public FilesControllerTests(BulletinBoardWebApplicationFactory factory) : base(factory)
    {
    }

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

    [Fact]
    public async Task DownloadFile_WhenExists_ReturnsFile()
    {
        // Arrange
        // 1. Авторизуемся и ЗАГРУЖАЕМ файл через API
        var client = CreateClient();
        var (token, _) = await AuthenticateAsUserAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();
        var fileBytes = new byte[] { 1, 2, 3, 4, 5 }; // Тестовые байты
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "download-test.jpg");

        var uploadResponse = await client.PostAsync("/api/files", content);
        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<FileUploadResultDto>();
        var fileId = uploadResult!.FileId;

        // Act
        // 2. Теперь СКАЧИВАЕМ этот реально существующий файл
        var response = await client.GetAsync($"/api/files/{fileId}/download");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/jpeg");
        var downloadedBytes = await response.Content.ReadAsByteArrayAsync();
        downloadedBytes.Should().BeEquivalentTo(fileBytes); // Сравниваем содержимое
    }

    [Fact]
    public async Task DeleteFile_AsAdmin_DeletesSuccessfully()
    {
        // Arrange
        var userClient = CreateClient();
        var (userToken, _) = await AuthenticateAsUserAsync(userClient);
        userClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
        
        var fileBytes = new byte[] { 1, 2, 3 };
        var fileContent = new ByteArrayContent(fileBytes);

        // Устанавливаем Content-Type для самого контента
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

        // Создаем multipart-контент
        using var content = new MultipartFormDataContent();

        // Добавляем контент файла. `name` ("file") должно совпадать с именем параметра в контроллере.
        // `fileName` ("to-delete.jpg") используется контроллером для определения расширения.
        content.Add(fileContent, "file", "to-delete.jpg");

        var uploadResponse = await userClient.PostAsync("/api/files", content);

        if (!uploadResponse.IsSuccessStatusCode)
        {
            var error = await uploadResponse.Content.ReadAsStringAsync();
            Assert.Fail($"Загрузка файла провалилась со статусом {uploadResponse.StatusCode}. Ответ: {error}");
        }

        var uploadResult = await uploadResponse.Content.ReadFromJsonAsync<FileUploadResultDto>();
        var fileId = uploadResult!.FileId;

        // ... остальная часть теста (авторизация админа и удаление) ...
        var adminClient = CreateClient();
        var (adminToken, _) = await AuthenticateAsAdminAsync(adminClient);
        adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await adminClient.DeleteAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await adminClient.GetAsync($"/api/files/{fileId}/info");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}