using InvoiceSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceSaaS.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        builder.Property(c => c.Document).HasColumnName("document").HasMaxLength(20).IsRequired();
        builder.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(c => c.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(c => c.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(c => c.Country).HasColumnName("country").HasMaxLength(100);
        builder.Property(c => c.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("active");
        builder.Property(c => c.CreatedBy).HasColumnName("created_by");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder.HasIndex(c => new { c.CompanyId, c.Document }).IsUnique().HasFilter("is_deleted = false");

        builder.HasOne(c => c.Company).WithMany(co => co.Customers).HasForeignKey(c => c.CompanyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.CreatedByUser).WithMany().HasForeignKey(c => c.CreatedBy).IsRequired(false);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
