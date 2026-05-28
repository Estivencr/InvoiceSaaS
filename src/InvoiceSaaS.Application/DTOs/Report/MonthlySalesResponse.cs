namespace InvoiceSaaS.Application.DTOs.Report;

public class MonthlySalesResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
}

public class TopCustomerResponse
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class RevenueByPeriodResponse
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PaidRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int PendingInvoices { get; set; }
    public int CancelledInvoices { get; set; }
}
