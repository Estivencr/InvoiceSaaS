using InvoiceSaaS.Application.DTOs.Report;

namespace InvoiceSaaS.Application.Services;

public interface IReportService
{
    Task<DashboardResponse> GetDashboardAsync(Guid companyId, CancellationToken ct = default);
    Task<IEnumerable<MonthlySalesResponse>> GetMonthlySalesAsync(Guid companyId, int months = 12, CancellationToken ct = default);
    Task<IEnumerable<TopCustomerResponse>> GetTopCustomersAsync(Guid companyId, int top = 10, CancellationToken ct = default);
    Task<RevenueByPeriodResponse> GetRevenueByPeriodAsync(Guid companyId, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default);
    Task<byte[]> ExportInvoicesToCsvAsync(Guid companyId, DateTime? dateFrom, DateTime? dateTo, CancellationToken ct = default);
}
