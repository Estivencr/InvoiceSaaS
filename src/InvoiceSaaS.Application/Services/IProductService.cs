using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Product;

namespace InvoiceSaaS.Application.Services;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetAllAsync(Guid companyId, PaginationParams p, CancellationToken ct = default);
    Task<ProductResponse> GetByIdAsync(Guid companyId, Guid id, CancellationToken ct = default);
    Task<IEnumerable<ProductResponse>> SearchAsync(Guid companyId, string term, CancellationToken ct = default);
    Task<ProductResponse> CreateAsync(Guid companyId, CreateProductRequest request, CancellationToken ct = default);
    Task<ProductResponse> UpdateAsync(Guid companyId, Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid companyId, Guid id, CancellationToken ct = default);
}
