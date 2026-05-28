using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Invoice;

namespace InvoiceSaaS.Application.Services;

public interface IInvoiceService
{
    Task<PagedResult<InvoiceResponse>> GetAllAsync(Guid companyId, PaginationParams pagination, CancellationToken ct = default);
    Task<InvoiceResponse> GetByIdAsync(Guid companyId, Guid id, CancellationToken ct = default);
    Task<InvoiceResponse> CreateAsync(Guid companyId, Guid userId, CreateInvoiceRequest request, CancellationToken ct = default);
    Task<InvoiceResponse> UpdateAsync(Guid companyId, Guid id, UpdateInvoiceRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid companyId, Guid id, CancellationToken ct = default);
    Task<InvoiceResponse> UpdateStatusAsync(Guid companyId, Guid id, UpdateInvoiceStatusRequest request, CancellationToken ct = default);
    Task<InvoiceResponse> AddDetailAsync(Guid companyId, Guid invoiceId, InvoiceDetailRequest request, CancellationToken ct = default);
    Task RemoveDetailAsync(Guid companyId, Guid invoiceId, Guid detailId, CancellationToken ct = default);
}
