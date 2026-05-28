using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Customer;

namespace InvoiceSaaS.Application.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerResponse>> GetAllAsync(Guid companyId, PaginationParams pagination, CancellationToken ct = default);
    Task<CustomerResponse> GetByIdAsync(Guid companyId, Guid id, CancellationToken ct = default);
    Task<CustomerResponse> CreateAsync(Guid companyId, Guid userId, CreateCustomerRequest request, CancellationToken ct = default);
    Task<CustomerResponse> UpdateAsync(Guid companyId, Guid id, UpdateCustomerRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid companyId, Guid id, CancellationToken ct = default);
    Task<IEnumerable<CustomerResponse>> SearchAsync(Guid companyId, string term, CancellationToken ct = default);
}
