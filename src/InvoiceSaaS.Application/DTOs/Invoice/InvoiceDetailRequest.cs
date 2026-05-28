namespace InvoiceSaaS.Application.DTOs.Invoice;

public class InvoiceDetailRequest
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int Sequence { get; set; }
}
