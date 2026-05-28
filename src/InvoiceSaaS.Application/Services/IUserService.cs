using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.User;

namespace InvoiceSaaS.Application.Services;

public interface IUserService
{
    Task<PagedResult<UserResponse>> GetAllAsync(Guid companyId, PaginationParams pagination, CancellationToken ct = default);
    Task<UserResponse> GetByIdAsync(Guid companyId, Guid id, CancellationToken ct = default);
    Task<UserResponse> CreateAsync(Guid companyId, CreateUserRequest request, CancellationToken ct = default);
    Task<UserResponse> UpdateAsync(Guid companyId, Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid companyId, Guid id, CancellationToken ct = default);
    Task<UserResponse> ChangeRoleAsync(Guid companyId, Guid id, ChangeRoleRequest request, CancellationToken ct = default);
    Task<UserResponse> ToggleStatusAsync(Guid companyId, Guid id, CancellationToken ct = default);
}
