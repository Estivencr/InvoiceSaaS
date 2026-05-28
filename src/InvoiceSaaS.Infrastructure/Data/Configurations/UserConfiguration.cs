using InvoiceSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceSaaS.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        builder.Property(u => u.FirstName).HasColumnName("first_name").HasMaxLength(100);
        builder.Property(u => u.LastName).HasColumnName("last_name").HasMaxLength(100);
        builder.Property(u => u.RefreshToken).HasColumnName("refresh_token").HasMaxLength(500);
        builder.Property(u => u.RefreshTokenExpiry).HasColumnName("refresh_token_expiry");
        builder.Property(u => u.LastLogin).HasColumnName("last_login");
        builder.Property(u => u.FailedLoginAttempts).HasColumnName("failed_login_attempts").HasDefaultValue(0);
        builder.Property(u => u.LockedUntil).HasColumnName("locked_until");
        builder.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(u => u.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder.HasIndex(u => new { u.CompanyId, u.Email }).IsUnique();

        builder.HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
