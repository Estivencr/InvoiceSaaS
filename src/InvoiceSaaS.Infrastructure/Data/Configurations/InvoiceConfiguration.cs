using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceSaaS.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(i => i.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(50).IsRequired();
        builder.Property(i => i.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(i => i.CreatedById).HasColumnName("created_by").IsRequired();
        builder.Property(i => i.IssueDate).HasColumnName("issue_date").IsRequired();
        builder.Property(i => i.DueDate).HasColumnName("due_date");
        builder.Property(i => i.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.TaxRate).HasColumnName("tax_rate").HasPrecision(5, 2).HasDefaultValue(19.00m);
        builder.Property(i => i.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.Total).HasColumnName("total").HasPrecision(18, 2).IsRequired();
        builder.Property(i => i.Status).HasColumnName("status").HasConversion<string>().HasDefaultValue(InvoiceStatus.Pending);
        builder.Property(i => i.PaymentDate).HasColumnName("payment_date");
        builder.Property(i => i.Notes).HasColumnName("notes");
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(i => i.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        builder.HasIndex(i => new { i.CompanyId, i.InvoiceNumber }).IsUnique();
        builder.HasIndex(i => new { i.CompanyId, i.Status });
        builder.HasIndex(i => i.CustomerId);

        builder.HasOne(i => i.Company).WithMany(c => c.Invoices).HasForeignKey(i => i.CompanyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Customer).WithMany(cu => cu.Invoices).HasForeignKey(i => i.CustomerId);
        builder.HasOne(i => i.CreatedBy).WithMany().HasForeignKey(i => i.CreatedById);

        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}

public class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
{
    public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
    {
        builder.ToTable("invoice_details");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.InvoiceId).HasColumnName("invoice_id").IsRequired();
        builder.Property(d => d.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(d => d.Quantity).HasColumnName("quantity").HasPrecision(18, 2).IsRequired();
        builder.Property(d => d.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2).IsRequired();
        builder.Property(d => d.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(d => d.Sequence).HasColumnName("sequence").IsRequired();
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(d => d.InvoiceId);

        builder.HasOne(d => d.Invoice).WithMany(i => i.Details).HasForeignKey(d => d.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.CompanyId).HasColumnName("company_id").IsRequired();
        builder.Property(a => a.EntityType).HasColumnName("entity_type").HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasColumnName("entity_id").IsRequired();
        builder.Property(a => a.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
        builder.Property(a => a.OldValue).HasColumnName("old_value");
        builder.Property(a => a.NewValue).HasColumnName("new_value");
        builder.Property(a => a.UserId).HasColumnName("user_id");
        builder.Property(a => a.Timestamp).HasColumnName("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(a => a.IpAddress).HasColumnName("ip_address").HasMaxLength(45);

        builder.HasIndex(a => new { a.CompanyId, a.EntityType, a.EntityId });

        builder.HasOne(a => a.Company).WithMany(c => c.AuditLogs).HasForeignKey(a => a.CompanyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).IsRequired(false);
    }
}
