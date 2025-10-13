using BulletinBoard.Application.Common;
using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Contracts.Categories;
using BulletinBoard.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Hosts.Api.Controllers;

/// <summary>
/// Контроллер для работы с категориями.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    /// <summary>Инициализирует контроллер категориями.</summary>
    /// <param name="service">Прикладной сервис категорий.</param>
    public CategoriesController(ICategoryService service) => _service = service;

    /// <summary>
    /// Возвращает корневые категории (без родителя).
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список категорий верхнего уровня.</returns>
    [HttpGet("roots")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetRoots(CancellationToken cancellationToken = default)
        => Ok(await _service.GetRootsAsync(cancellationToken));

    /// <summary>
    /// Возвращает дочерние категории указанного родителя с пагинацией.
    /// </summary>
    /// <param name="parentId">Идентификатор родительской категории.</param>
    /// <param name="page">Параметры пагинации (номер страницы и размер).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Пагинированный список дочерних категорий.</returns>
    [HttpGet("{parentId:guid}/children")]
    [ProducesResponseType(typeof(PagedResult<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CategoryDto>>> GetChildren(
        Guid parentId,
        [FromQuery(Name = "")] PageRequest page,
        CancellationToken cancellationToken = default)
    {
        page.ThrowIfInvalid();
        var result = await _service.GetChildrenAsync(parentId, page, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Создаёт корневую категорию.
    /// </summary>
    /// <param name="name">Название категории (в query).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданная категория.</returns>
    [HttpPost("root")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> CreateRoot([FromQuery] string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");
        var created = await _service.CreateRootAsync(name, cancellationToken);
        return CreatedAtAction(nameof(GetRoots), null, created);
    }

    /// <summary>
    /// Создаёт дочернюю категорию.
    /// </summary>
    /// <param name="parentId">Идентификатор родительской категории.</param>
    /// <param name="name">Название категории (в query).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданная категория.</returns>
    [HttpPost("{parentId:guid}/child")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> CreateChild(Guid parentId, [FromQuery] string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");
        var created = await _service.CreateChildAsync(parentId, name, cancellationToken);
        // Можно вернуть Created, однако точной GetById нет; вернём 201 без location-лупа:
        return Created(string.Empty, created);
    }

    /// <summary>
    /// Удаляет категорию.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>204 при успехе, 404 если не найдена.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var ok = await _service.DeleteAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
