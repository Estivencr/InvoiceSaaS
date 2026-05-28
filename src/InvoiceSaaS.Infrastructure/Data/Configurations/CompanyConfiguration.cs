using InvoiceSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceSaaS.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.HasIndex(c => c.Email).IsUnique();
        builder.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(c => c.Document).HasColumnName("document").HasMaxLength(20);
        builder.HasIndex(c => c.Document).IsUnique().HasFilter("document IS NOT NULL");
        builder.Property(c => c.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(c => c.Country).HasColumnName("country").HasMaxLength(100);
        builder.Property(c => c.SubscriptionPlan).HasColumnName("subscription_plan").HasMaxLength(50).HasDefaultValue("free");
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
