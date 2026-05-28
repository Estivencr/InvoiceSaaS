namespace InvoiceSaaS.Application.DTOs.Invoice;

public class CreateInvoiceRequest
{
    public Guid CustomerId { get; set; }
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public decimal TaxRate { get; set; } = 19.00m;
    public string? Notes { get; set; }
    public List<InvoiceDetailRequest> Details { get; set; } = new();
}
