using InvoiceSaaS.Domain.Enums;
using InvoiceSaaS.Domain.Interfaces;

namespace InvoiceSaaS.Domain.Entities;

public class Invoice : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; } = 19.00m;
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    public Company Company { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
    public ICollection<InvoiceDetail> Details { get; set; } = new List<InvoiceDetail>();

    public void RecalculateTotals()
    {
        Subtotal = Details.Sum(d => d.Amount);
        TaxAmount = Math.Round(Subtotal * TaxRate / 100, 2);
        Total = Subtotal + TaxAmount;
    }
}
