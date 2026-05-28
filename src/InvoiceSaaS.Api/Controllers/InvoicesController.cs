using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Invoice;
using InvoiceSaaS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSaaS.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICurrentUserService _currentUser;

    public InvoicesController(IInvoiceService invoiceService, ICurrentUserService currentUser)
    {
        _invoiceService = invoiceService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _invoiceService.GetAllAsync(_currentUser.CompanyId, pagination, ct);
        return Ok(ApiResponse<PagedResult<InvoiceResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _invoiceService.GetByIdAsync(_currentUser.CompanyId, id, ct);
        return Ok(ApiResponse<InvoiceResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request, CancellationToken ct)
    {
        var result = await _invoiceService.CreateAsync(_currentUser.CompanyId, _currentUser.UserId, request, ct);
        return Created(string.Empty, ApiResponse<InvoiceResponse>.Ok(result, "Invoice created."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceRequest request, CancellationToken ct)
    {
        var result = await _invoiceService.UpdateAsync(_currentUser.CompanyId, id, request, ct);
        return Ok(ApiResponse<InvoiceResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _invoiceService.DeleteAsync(_currentUser.CompanyId, id, ct);
        return Ok(ApiResponse.Ok("Invoice deleted."));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateInvoiceStatusRequest request, CancellationToken ct)
    {
        var result = await _invoiceService.UpdateStatusAsync(_currentUser.CompanyId, id, request, ct);
        return Ok(ApiResponse<InvoiceResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/details")]
    public async Task<IActionResult> AddDetail(Guid id, [FromBody] InvoiceDetailRequest request, CancellationToken ct)
    {
        var result = await _invoiceService.AddDetailAsync(_currentUser.CompanyId, id, request, ct);
        return Ok(ApiResponse<InvoiceResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}/details/{detailId:guid}")]
    public async Task<IActionResult> RemoveDetail(Guid id, Guid detailId, CancellationToken ct)
    {
        await _invoiceService.RemoveDetailAsync(_currentUser.CompanyId, id, detailId, ct);
        return Ok(ApiResponse.Ok("Detail removed."));
    }
}
