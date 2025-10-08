using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Contracts.Users;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Hosts.Api.Controllers;

/// <summary>
/// Контроллер для работы с пользователями.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _service;

    /// <summary>Инициализирует контроллер пользователями.</summary>
    /// <param name="service">Прикладной сервис пользователей.</param>
    public UsersController(IUserService service) => _service = service;

    /// <summary>
    /// Возвращает пользователя по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>DTO пользователя или 404.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await _service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>
    /// Создаёт пользователя.
    /// </summary>
    /// <param name="request">Данные для создания пользователя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Созданный пользователь.</returns>
    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var created = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Обновляет данные пользователя.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="request">Изменяемые поля.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обновлённый пользователь либо 404.</returns>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserDto request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var updated = await _service.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
}
