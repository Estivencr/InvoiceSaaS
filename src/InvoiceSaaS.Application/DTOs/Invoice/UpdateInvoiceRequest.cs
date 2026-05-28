namespace InvoiceSaaS.Application.DTOs.Invoice;

public class UpdateInvoiceRequest
{
    public Guid CustomerId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal TaxRate { get; set; } = 19.00m;
    public string? Notes { get; set; }
    public List<InvoiceDetailRequest> Details { get; set; } = new();
}
