using System.Security.Claims;
using BulletinBoard.Application.Abstractions;
using BulletinBoard.Application.Common;
using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Contracts.Auth;
using BulletinBoard.Contracts.Common;
using BulletinBoard.Contracts.Users;
using Microsoft.AspNetCore.Authorization;
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
    private readonly ICurrentUserService _currentUserService;

    /// <summary>Инициализирует контроллер пользователями.</summary>
    /// <param name="service">Прикладной сервис пользователей.</param>
    /// <param name="currentUserService">Сервис авторизованного пользователя.</param>
    public UsersController(IUserService service, ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

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

    // /// <summary>
    // /// Создаёт пользователя.
    // /// </summary>
    // /// <param name="request">Данные для создания пользователя.</param>
    // /// <param name="cancellationToken">Токен отмены.</param>
    // /// <returns>Созданный пользователь.</returns>
    // [HttpPost]
    // [Consumes("application/json")]
    // [Produces("application/json")]
    // [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto request, CancellationToken cancellationToken = default)
    // {
    //     var created = await _service.CreateAsync(request, cancellationToken);
    //     return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    // }

    /// <summary>
    /// Обновляет данные пользователя.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="request">Изменяемые поля.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обновлённый пользователь либо 404.</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserDto request, CancellationToken cancellationToken = default)
    {
        var updated = await _service.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }
    
    /// <summary>
    /// Возвращает список пользователей постранично.
    /// </summary>
    /// <param name="page">Номер страницы (по умолчанию 1) и размер страницы (по умолчанию 10).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Пагинированный список пользователей.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetPage(
        [FromQuery(Name = "")] PageRequest page,
        CancellationToken cancellationToken = default)
    {
        page.ThrowIfInvalid();
        var result = await _service.GetPageAsync(page, cancellationToken);
        return Ok(result);
    }
    
    /// <summary>
    /// Возвращает информацию о текущем авторизованном пользователе.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserInfoDto>> GetCurrentUser(
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }
        
        var user = await _service.GetByIdAsync(userId.Value, cancellationToken);
        
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new UserInfoDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
        });
    }
}
