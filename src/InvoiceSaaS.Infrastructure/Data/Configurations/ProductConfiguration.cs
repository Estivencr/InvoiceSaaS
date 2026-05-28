using InvoiceSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceSaaS.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(p => p.SKU).HasColumnName("sku").HasMaxLength(100);
        builder.Property(p => p.Category).HasColumnName("category").HasMaxLength(100);
        builder.Property(p => p.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.Stock).HasColumnName("stock").HasDefaultValue(0);
        builder.Property(p => p.Unit).HasColumnName("unit").HasMaxLength(50).HasDefaultValue("unit");
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.CompanyId, p.SKU })
            .HasDatabaseName("ix_products_company_sku")
            .IsUnique()
            .HasFilter("sku IS NOT NULL AND is_deleted = false");
    }
}
