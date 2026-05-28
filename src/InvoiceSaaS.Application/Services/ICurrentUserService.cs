namespace InvoiceSaaS.Application.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid CompanyId { get; }
    string Email { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
}
