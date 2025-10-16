using System.Security.Claims;
using BulletinBoard.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BulletinBoard.Infrastructure.Services;

/// <summary>
/// Реализация <see cref="ICurrentUserService"/> для работы с текущим авторизованным пользователем.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Инициализирует сервис текущего пользователя.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor для доступа к HttpContext.</param>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Если claim отсутствует или невалиден
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }

    /// <inheritdoc />
    public bool IsAdmin()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }
        
        return httpContext.User.IsInRole("Admin");
    }

    /// <inheritdoc />
    public bool IsOwnerOrAdmin(Guid resourceOwnerId)
    {
        if (IsAdmin())
        {
            return true;
        }
        
        var currentUserId = GetCurrentUserId();
        
        if (currentUserId == null)
        {
            return false;
        }
        
        return currentUserId.Value == resourceOwnerId;
    }
}