using System.Security.Claims;
using BulletinBoard.Application.Contexts.Auth;
using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulletinBoard.Hosts.Api.Controllers;

/// <summary>
/// Контроллер для аутентификации и авторизации.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Инициализирует контроллер аутентификации.
    /// </summary>
    /// <param name="authService">Сервис аутентификации.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Регистрирует нового пользователя.
    /// </summary>
    /// <param name="request">Данные для регистрации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Токены доступа.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponseDto>> Register(
        [FromBody] RegisterDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Register), response);
    }

    /// <summary>
    /// Выполняет вход пользователя.
    /// </summary>
    /// <param name="request">Данные для входа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Токены доступа.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Обновляет access token с помощью refresh token.
    /// </summary>
    /// <param name="request">Refresh token.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Новые токены.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken(
        [FromBody] RefreshTokenDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await _authService.RefreshTokenAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Выполняет выход пользователя (отзывает refresh token).
    /// </summary>
    /// <param name="request">Refresh token для отзыва.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>204 No Content.</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenDto request,
        CancellationToken cancellationToken = default)
    {
        await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }
    
    // /// <summary>
    // /// Возвращает информацию о текущем авторизованном пользователе.
    // /// </summary>
    // [HttpGet("me")]
    // [Authorize]
    // [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<UserInfoDto>> GetCurrentUser(
    //     [FromServices] IUserRepository userRepository,
    //     CancellationToken cancellationToken = default)
    // {
    //     var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //     if (string.IsNullOrEmpty(userIdClaim))
    //     {
    //         return Unauthorized();
    //     }
    //
    //     var userId = Guid.Parse(userIdClaim);
    //     var user = await userRepository.GetByIdAsync(userId, cancellationToken);
    //
    //     if (user == null)
    //     {
    //         return NotFound();
    //     }
    //
    //     return Ok(new UserInfoDto
    //     {
    //         Id = user.Id,
    //         DisplayName = user.DisplayName,
    //         Email = user.Email,
    //         Role = user.Role.ToString()
    //     });
    // }
}