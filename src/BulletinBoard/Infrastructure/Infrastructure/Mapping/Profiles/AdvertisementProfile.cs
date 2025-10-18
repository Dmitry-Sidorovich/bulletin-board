using AutoMapper;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Domain.Entities;
using BulletinBoard.Domain.ValueObjects;

namespace BulletinBoard.Infrastructure.Mapping.Profiles;

/// <summary>Профили маппинга объявлений и связанных DTO.</summary>
public sealed class AdvertisementProfile : Profile
{
    /// <summary>
    /// Инициализирует экземпляр <see cref="AdvertisementProfile"/>.
    /// </summary>
    public AdvertisementProfile()
    {
        // Domain → DTO
        CreateMap<Advertisement, AdvertisementDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (AdStatusDto)src.Status))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Files, opt => opt.Ignore());
        
        // Value Object → DTO
        CreateMap<Contact, ContactDto>();
    }
}