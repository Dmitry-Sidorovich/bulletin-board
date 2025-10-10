using BulletinBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BulletinBoard.Infrastructure.DataAccess.Configurations;

/// <summary>
/// Конфигурация EF Core для сущности AdvertisementFile (junction table).
/// </summary>
public sealed class AdvertisementFileConfiguration : IEntityTypeConfiguration<AdvertisementFile>
{
    public void Configure(EntityTypeBuilder<AdvertisementFile> builder)
    {
        builder.ToTable("advertisement_files");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AdvertisementId)
            .IsRequired();

        builder.Property(x => x.FileId)
            .IsRequired();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.AdvertisementId);
        builder.HasIndex(x => x.FileId);
        builder.HasIndex(x => new { x.AdvertisementId, x.Order });
    }
}