using BulletinBoard.Application.Contexts.Files.Services;
using BulletinBoard.Contracts.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Hosts.Api.Controllers;

/// <summary>
/// Контроллер для работы с файлами.
/// </summary>
[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    /// <summary>
    /// Инициализирует контроллер файлов.
    /// </summary>
    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Загрузить файл.
    /// </summary>
    /// <param name="file">Файл для загрузки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат загрузки с ID и URL файла.</returns>
    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileUploadResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FileUploadResultDto>> Upload(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не выбран или пустой.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest("Разрешены только форматы: JPG, PNG, WebP.");

        const long maxFileSize = 5 * 1024 * 1024;
        if (file.Length > maxFileSize)
            return BadRequest("Максимальный размер файла: 5 MB.");

        await using var stream = file.OpenReadStream();
        var result = await _fileService.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);

        return CreatedAtAction(nameof(GetInfo), new { id = result.FileId }, result);
    }

    /// <summary>
    /// Получить информацию о файле.
    /// </summary>
    /// <param name="id">ID файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Информация о файле.</returns>
    [HttpGet("{id:guid}/info")]
    [ProducesResponseType(typeof(FileInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FileInfoDto>> GetInfo(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _fileService.GetInfoAsync(id, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Скачать файл.
    /// </summary>
    /// <param name="id">ID файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Файл для скачивания.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _fileService.DownloadAsync(id, cancellationToken);
        if (result == null)
            return NotFound();

        var (stream, fileName, contentType) = result.Value;
        return File(stream, contentType, fileName);
    }

    /// <summary>
    /// Удалить файл.
    /// </summary>
    /// <param name="id">ID файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>204 No Content при успехе.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _fileService.DeleteAsync(id, cancellationToken);
        return result ? NoContent() : NotFound();
    }
}