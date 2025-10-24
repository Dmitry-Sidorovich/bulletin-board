using BulletinBoard.Application.Common;
using BulletinBoard.Application.Contexts.Categories;
using BulletinBoard.Contracts.Categories;
using BulletinBoard.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
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
    /// Возвращает категорию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор категории.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Категория или 404, если не найдена.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _service.GetByIdAsync(id, cancellationToken);
        return category is not null ? Ok(category) : NotFound();
    }

    /// <summary>
    /// Создаёт корневую категорию.
    /// </summary>
    /// <param name="request">Данные для создания категории.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданная категория.</returns>
    [HttpPost("root")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryDto>> CreateRoot([FromBody] CreateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var created = await _service.CreateRootAsync(request.Name, cancellationToken);
        return CreatedAtAction(nameof(GetRoots), null, created);
    }

    /// <summary>
    /// Создаёт дочернюю категорию.
    /// </summary>
    /// <param name="parentId">Идентификатор родительской категории.</param>
    /// <param name="request">Данные для создания категории.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданная категория.</returns>
    [HttpPost("{parentId:guid}/child")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> CreateChild(Guid parentId, [FromBody] CreateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var created = await _service.CreateChildAsync(parentId, request.Name, cancellationToken);
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
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var ok = await _service.DeleteAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
