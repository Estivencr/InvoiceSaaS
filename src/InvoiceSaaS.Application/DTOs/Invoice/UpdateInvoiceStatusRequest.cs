using InvoiceSaaS.Domain.Enums;

namespace InvoiceSaaS.Application.DTOs.Invoice;

public class UpdateInvoiceStatusRequest
{
    public InvoiceStatus Status { get; set; }
    public DateTime? PaymentDate { get; set; }
}
