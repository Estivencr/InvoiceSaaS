using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Report;
using InvoiceSaaS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSaaS.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ICurrentUserService _currentUser;

    public ReportsController(IReportService reportService, ICurrentUserService currentUser)
    {
        _reportService = reportService;
        _currentUser = currentUser;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var result = await _reportService.GetDashboardAsync(_currentUser.CompanyId, ct);
        return Ok(ApiResponse<DashboardResponse>.Ok(result));
    }

    [HttpGet("monthly-sales")]
    public async Task<IActionResult> MonthlySales([FromQuery] int months = 12, CancellationToken ct = default)
    {
        var result = await _reportService.GetMonthlySalesAsync(_currentUser.CompanyId, months, ct);
        return Ok(ApiResponse<IEnumerable<MonthlySalesResponse>>.Ok(result));
    }

    [HttpGet("top-customers")]
    public async Task<IActionResult> TopCustomers([FromQuery] int top = 10, CancellationToken ct = default)
    {
        var result = await _reportService.GetTopCustomersAsync(_currentUser.CompanyId, top, ct);
        return Ok(ApiResponse<IEnumerable<TopCustomerResponse>>.Ok(result));
    }

    [HttpGet("revenue-by-period")]
    public async Task<IActionResult> RevenueByPeriod(
        [FromQuery] DateTime dateFrom,
        [FromQuery] DateTime dateTo,
        CancellationToken ct)
    {
        var result = await _reportService.GetRevenueByPeriodAsync(_currentUser.CompanyId, dateFrom, dateTo, ct);
        return Ok(ApiResponse<RevenueByPeriodResponse>.Ok(result));
    }

    [HttpGet("export-invoices")]
    public async Task<IActionResult> ExportInvoices(
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken ct)
    {
        var csvBytes = await _reportService.ExportInvoicesToCsvAsync(_currentUser.CompanyId, dateFrom, dateTo, ct);
        var fileName = $"invoices-{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(csvBytes, "text/csv", fileName);
    }
}
