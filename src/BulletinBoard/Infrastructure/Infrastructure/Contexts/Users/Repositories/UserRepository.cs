using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Infrastructure.Contexts.Users.Repositories;

/// <inheritdoc />
public sealed class UserRepository : IUserRepository
{
    /// <inheritdoc />
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}