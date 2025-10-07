using BulletinBoard.Application.Contexts.Users;
using BulletinBoard.Contracts.Users;

namespace BulletinBoard.Infrastructure.Contexts.Users.Repositories;

/// <inheritdoc />
public sealed class UserReadRepository: IUserReadRepository
{
    /// <inheritdoc />
    public Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}