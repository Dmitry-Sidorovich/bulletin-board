using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Infrastructure.DataAccess.Db;
using Microsoft.EntityFrameworkCore;

namespace BulletinBoard.Infrastructure.Contexts.Users.Repositories;

/// <inheritdoc />
public sealed class UserRepository : IUserRepository
{
    private readonly BulletinBoardDbContext _dbContext;

    /// <summary>
    /// Инициализирует экземпляр <see cref="UserRepository"/>.
    /// </summary>
    /// <param name="dbContext">Контекст базы данных.</param>
    public UserRepository(BulletinBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        _dbContext.Users.Update(user);
        
        return Task.CompletedTask;
    }
    
    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}