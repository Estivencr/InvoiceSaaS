using System.Security.Claims;
using InvoiceSaaS.Application.Services;
using Microsoft.AspNetCore.Http;

namespace InvoiceSaaS.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid UserId => Guid.Parse(User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue("sub")
        ?? throw new InvalidOperationException("User is not authenticated."));

    public Guid CompanyId => Guid.Parse(User?.FindFirstValue("company_id")
        ?? throw new InvalidOperationException("Company claim not found."));

    public string Email => User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue("email")
        ?? throw new InvalidOperationException("Email claim not found.");

    public IEnumerable<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value)
        ?? Enumerable.Empty<string>();

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
