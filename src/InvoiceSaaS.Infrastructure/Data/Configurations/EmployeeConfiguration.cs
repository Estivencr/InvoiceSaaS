using InvoiceSaaS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceSaaS.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(e => e.Position).HasColumnName("position").HasMaxLength(100);
        builder.Property(e => e.RoleId).HasColumnName("role_id").IsRequired();
        builder.Property(e => e.HireDate).HasColumnName("hire_date");
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("active");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder.HasIndex(e => new { e.CompanyId, e.Email }).IsUnique().HasFilter("is_deleted = false");

        builder.HasOne(e => e.Company).WithMany(c => c.Employees).HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Role).WithMany(r => r.Employees).HasForeignKey(e => e.RoleId);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
