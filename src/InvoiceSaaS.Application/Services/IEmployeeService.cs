using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Employee;

namespace InvoiceSaaS.Application.Services;

public interface IEmployeeService
{
    Task<IEnumerable<RoleResponse>> GetRolesAsync(Guid companyId, CancellationToken ct = default);
    Task<PagedResult<EmployeeResponse>> GetAllAsync(Guid companyId, PaginationParams pagination, CancellationToken ct = default);
    Task<EmployeeResponse> GetByIdAsync(Guid companyId, Guid id, CancellationToken ct = default);
    Task<IEnumerable<EmployeeResponse>> GetByRoleAsync(Guid companyId, Guid roleId, CancellationToken ct = default);
    Task<EmployeeResponse> CreateAsync(Guid companyId, CreateEmployeeRequest request, CancellationToken ct = default);
    Task<EmployeeResponse> UpdateAsync(Guid companyId, Guid id, UpdateEmployeeRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid companyId, Guid id, CancellationToken ct = default);
}
