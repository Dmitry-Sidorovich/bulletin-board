using AutoMapper;
using BulletinBoard.Contracts.Categories;
using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Infrastructure.Mapping.Profiles;

/// <summary>Профиль маппинга категорий.</summary>
public sealed class CategoryProfile : Profile
{
    /// <summary>
    /// Инициализирует экземпляр <see cref="CategoryProfile"/>.
    /// </summary>
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>();
    }
}