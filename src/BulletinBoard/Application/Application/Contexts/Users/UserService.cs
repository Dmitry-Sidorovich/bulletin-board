using BulletinBoard.Contracts.Common;
using BulletinBoard.Contracts.Users;

namespace BulletinBoard.Application.Contexts.Users;

/// <inheritdoc />
public class UserService : IUserService
{
    /// <inheritdoc />
    public Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<PagedResult<UserDto>> GetPageAsync(string? nameFilter, PageRequest page, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}