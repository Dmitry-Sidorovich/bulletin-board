using BulletinBoard.Domain.Entities;
using BulletinBoard.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BulletinBoard.Infrastructure.DataAccess.Configurations;

/// <summary>Конфигурация EF для <see cref="User"/>.</summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Конфигурирует сущность <see cref="User"/> для Entity Framework.
    /// </summary>
    /// <param name="builder">Построитель конфигурации сущности.</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).IsRequired();
        
        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Phone)
            .HasMaxLength(50);
        
        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<UserRole>(v));

        builder.HasMany(x => x.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}