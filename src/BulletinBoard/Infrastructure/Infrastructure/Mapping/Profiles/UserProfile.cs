using AutoMapper;
using BulletinBoard.Contracts.Users;
using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Infrastructure.Mapping.Profiles;

/// <summary>Профиль маппинга пользователей.</summary>
public sealed class UserProfile : Profile
{
    /// <summary>
    /// Инициализирует экземпляр <see cref="UserProfile"/>.
    /// </summary>
    public UserProfile()
    {
        CreateMap<User, UserDto>();
    }
}