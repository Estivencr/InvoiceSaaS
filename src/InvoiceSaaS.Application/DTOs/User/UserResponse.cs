namespace InvoiceSaaS.Application.DTOs.User;

public class UserResponse
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
