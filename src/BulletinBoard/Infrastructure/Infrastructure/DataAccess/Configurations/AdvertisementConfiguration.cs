using BulletinBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BulletinBoard.Infrastructure.DataAccess.Configurations;

/// <summary>Конфигурация EF для <see cref="Advertisement"/>.</summary>
public sealed class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
{
    /// <summary>
    /// Конфигурирует сущность <see cref="Advertisement"/> для Entity Framework.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    public void Configure(EntityTypeBuilder<Advertisement> builder)
    {
        builder.ToTable("advertisements");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(a => a.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)"); 

        builder.Property(x => x.Status)
            .HasConversion<int>() // enum → int
            .IsRequired();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Value Object Contact (owned type)
        builder.OwnsOne(x => x.Contact, contact =>
        {
            contact.Property(c => c.Name)
                .HasColumnName("contact_name")
                .IsRequired()
                .HasMaxLength(100);

            contact.Property(c => c.Email)
                .HasColumnName("contact_email")
                .IsRequired()
                .HasMaxLength(200);

            contact.Property(c => c.Phone)
                .HasColumnName("contact_phone")
                .HasMaxLength(50);
        });
        
        builder.HasIndex(a => a.CategoryId);
        builder.HasIndex(a => a.AuthorId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Price); 
        builder.HasIndex(a => a.CreatedAt);
    }
}