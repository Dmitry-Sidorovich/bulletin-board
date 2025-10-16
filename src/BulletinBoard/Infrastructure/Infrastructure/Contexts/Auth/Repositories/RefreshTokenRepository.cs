using BulletinBoard.Application.Abstractions;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Infrastructure.Contexts.Auth.Repositories;

/// <summary>
/// Реализация <see cref="IRefreshTokenRepository"/>.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly BulletinBoardDbContext _context;

    /// <summary>
    /// Инициализирует репозиторий refresh токенов.
    /// </summary>
    /// <param name="context">Контекст БД.</param>
    public RefreshTokenRepository(BulletinBoardDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke();
        }
    }
}