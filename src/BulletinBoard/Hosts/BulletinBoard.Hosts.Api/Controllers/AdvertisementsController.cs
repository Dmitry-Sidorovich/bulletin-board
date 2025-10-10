using BulletinBoard.Application.Contexts.Advertisements;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Hosts.Api.Controllers;

/// <summary>
/// Контроллер для работы с объявлениями.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public sealed class AdvertisementsController : ControllerBase
{
    private readonly IAdvertisementService _service;

    /// <summary>Инициализирует контроллер объявлениями.</summary>
    /// <param name="service">Прикладной сервис объявлений.</param>
    public AdvertisementsController(IAdvertisementService service) => _service = service;

    /// <summary>
    /// Возвращает объявление по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>DTO объявления или код 404, если не найдено.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdvertisementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdvertisementDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await _service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>
    /// Возвращает объявления по категории с постраничным выводом.
    /// </summary>
    /// <param name="categoryId">Идентификатор категории.</param>
    /// <param name="page">Параметры пагинации (номер страницы и размер).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Пагинированный результат объявлений.</returns>
    [HttpGet("by-category/{categoryId:guid}")]
    [ProducesResponseType(typeof(PagedResult<AdvertisementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<AdvertisementDto>>> GetByCategory(
        Guid categoryId,
        [FromQuery] PageRequest page,
        CancellationToken cancellationToken = default)
    {
        if (page is null || page.Page < 1 || page.PageSize <= 0) return BadRequest("Invalid pagination.");
        var result = await _service.GetByCategoryAsync(categoryId, page, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Создаёт новое объявление.
    /// </summary>
    /// <param name="request">Данные для создания объявления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданное объявление и заголовок Location.</returns>
    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AdvertisementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdvertisementDto>> Create([FromBody] CreateAdvertisementDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var created = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Обновляет существующее объявление.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="request">Новые значения полей.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обновлённое объявление либо 404.</returns>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AdvertisementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdvertisementDto>> Update(Guid id, [FromBody] UpdateAdvertisementDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var updated = await _service.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// Изменяет статус объявления.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="request">Новый статус.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>204 при успехе, 404 если не найдено.</returns>
    [HttpPatch("{id:guid}/status")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeAdvertisementStatusDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var ok = await _service.ChangeStatusAsync(id, request, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Удаляет объявление.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>204 при успехе, 404 если не найдено.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var ok = await _service.DeleteAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
    
    /// <summary>
    /// Прикрепить файл к объявлению.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="fileId">Идентификатор файла (из query string).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>204 No Content при успехе.</returns>
    /// <response code="204">Файл успешно прикреплен к объявлению.</response>
    /// <response code="404">Объявление или файл не найдены.</response>
    [HttpPost("{id:guid}/attach-file")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AttachFile(
        Guid id,
        [FromQuery] Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.AttachFileAsync(id, fileId, cancellationToken);
        return result ? NoContent() : NotFound("Объявление или файл не найдены.");
    }

    /// <summary>
    /// Открепить файл от объявления.
    /// </summary>
    /// <param name="id">Идентификатор объявления.</param>
    /// <param name="fileId">Идентификатор файла (из query string).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>204 No Content при успехе.</returns>
    /// <response code="204">Файл успешно откреплен от объявления.</response>
    /// <response code="404">Привязка файла к объявлению не найдена.</response>
    [HttpDelete("{id:guid}/detach-file")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DetachFile(
        Guid id,
        [FromQuery] Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.DetachFileAsync(id, fileId, cancellationToken);
        return result ? NoContent() : NotFound("Привязка не найдена.");
    }
}
