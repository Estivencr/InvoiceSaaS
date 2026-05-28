using System.Text;
using AutoMapper;
using InvoiceSaaS.Application.DTOs.Invoice;
using InvoiceSaaS.Application.DTOs.Report;
using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Enums;
using InvoiceSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSaaS.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ReportService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<DashboardResponse> GetDashboardAsync(Guid companyId, CancellationToken ct = default)
    {
        var invoices = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Include(i => i.CreatedBy)
            .Include(i => i.Details)
            .Where(i => i.CompanyId == companyId && !i.IsDeleted)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);

        var paidInvoices = invoices.Where(i => i.Status == InvoiceStatus.Paid);

        return new DashboardResponse
        {
            TotalInvoices = invoices.Count,
            TotalRevenue = paidInvoices.Sum(i => i.Total),
            PendingAmount = invoices.Where(i => i.Status == InvoiceStatus.Pending).Sum(i => i.Total),
            ActiveCustomers = await _uow.Repository<Customer>()
                .CountAsync(c => c.CompanyId == companyId && !c.IsDeleted && c.Status == "active", ct),
            ThisMonthRevenue = paidInvoices.Where(i => i.IssueDate >= thisMonthStart).Sum(i => i.Total),
            InvoicesByStatus = new InvoicesByStatusDto
            {
                Pending = invoices.Count(i => i.Status == InvoiceStatus.Pending),
                Paid = invoices.Count(i => i.Status == InvoiceStatus.Paid),
                Cancelled = invoices.Count(i => i.Status == InvoiceStatus.Cancelled)
            },
            RecentInvoices = _mapper.Map<List<InvoiceResponse>>(
                invoices.OrderByDescending(i => i.CreatedAt).Take(10))
        };
    }

    public async Task<IEnumerable<MonthlySalesResponse>> GetMonthlySalesAsync(Guid companyId, int months = 12, CancellationToken ct = default)
    {
        var from = DateTime.UtcNow.AddMonths(-months + 1);
        var start = new DateTime(from.Year, from.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var invoices = await _uow.Repository<Invoice>().Query()
            .Where(i => i.CompanyId == companyId && !i.IsDeleted && i.IssueDate >= start)
            .ToListAsync(ct);

        return invoices
            .GroupBy(i => new { i.IssueDate.Year, i.IssueDate.Month })
            .Select(g => new MonthlySalesResponse
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                InvoiceCount = g.Count(),
                TotalRevenue = g.Sum(i => i.Total),
                PaidAmount = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Total),
                PendingAmount = g.Where(i => i.Status == InvoiceStatus.Pending).Sum(i => i.Total)
            })
            .OrderBy(r => r.Year).ThenBy(r => r.Month);
    }

    public async Task<IEnumerable<TopCustomerResponse>> GetTopCustomersAsync(Guid companyId, int top = 10, CancellationToken ct = default)
    {
        var invoices = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Where(i => i.CompanyId == companyId && !i.IsDeleted && i.Status == InvoiceStatus.Paid)
            .ToListAsync(ct);

        return invoices
            .GroupBy(i => new { i.CustomerId, i.Customer.Name, i.Customer.Document })
            .Select(g => new TopCustomerResponse
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.Name,
                CustomerDocument = g.Key.Document,
                InvoiceCount = g.Count(),
                TotalAmount = g.Sum(i => i.Total)
            })
            .OrderByDescending(r => r.TotalAmount)
            .Take(top);
    }

    public async Task<RevenueByPeriodResponse> GetRevenueByPeriodAsync(Guid companyId, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default)
    {
        var from = DateTime.SpecifyKind(dateFrom, DateTimeKind.Utc);
        var to = DateTime.SpecifyKind(dateTo, DateTimeKind.Utc);
        var invoices = await _uow.Repository<Invoice>().Query()
            .Where(i => i.CompanyId == companyId && !i.IsDeleted
                && i.IssueDate >= from && i.IssueDate <= to)
            .ToListAsync(ct);

        return new RevenueByPeriodResponse
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            TotalRevenue = invoices.Sum(i => i.Total),
            PaidRevenue = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Total),
            PendingRevenue = invoices.Where(i => i.Status == InvoiceStatus.Pending).Sum(i => i.Total),
            TotalInvoices = invoices.Count,
            PaidInvoices = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            PendingInvoices = invoices.Count(i => i.Status == InvoiceStatus.Pending),
            CancelledInvoices = invoices.Count(i => i.Status == InvoiceStatus.Cancelled)
        };
    }

    public async Task<byte[]> ExportInvoicesToCsvAsync(Guid companyId, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default)
    {
        var query = _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Where(i => i.CompanyId == companyId && !i.IsDeleted);

        if (dateFrom.HasValue) query = query.Where(i => i.IssueDate >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(i => i.IssueDate <= dateTo.Value);

        var invoices = await query.OrderByDescending(i => i.IssueDate).ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("InvoiceNumber,Customer,Document,IssueDate,DueDate,Subtotal,TaxRate,TaxAmount,Total,Status,PaymentDate");

        foreach (var i in invoices)
        {
            sb.AppendLine($"{i.InvoiceNumber},{EscapeCsv(i.Customer.Name)},{i.Customer.Document}," +
                          $"{i.IssueDate:yyyy-MM-dd},{i.DueDate:yyyy-MM-dd},{i.Subtotal}," +
                          $"{i.TaxRate},{i.TaxAmount},{i.Total},{i.Status},{i.PaymentDate:yyyy-MM-dd}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value) =>
        value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
