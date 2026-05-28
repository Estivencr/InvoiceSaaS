using InvoiceSaaS.Application.DTOs.Invoice;

namespace InvoiceSaaS.Application.DTOs.Report;

public class DashboardResponse
{
    public int TotalInvoices { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingAmount { get; set; }
    public int ActiveCustomers { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    public InvoicesByStatusDto InvoicesByStatus { get; set; } = new();
    public List<InvoiceResponse> RecentInvoices { get; set; } = new();
}

public class InvoicesByStatusDto
{
    public int Pending { get; set; }
    public int Paid { get; set; }
    public int Cancelled { get; set; }
}
