using InvoiceSaaS.Domain.Enums;

namespace InvoiceSaaS.Application.DTOs.Invoice;

public class InvoiceResponse
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<InvoiceDetailResponse> Details { get; set; } = new();
}
