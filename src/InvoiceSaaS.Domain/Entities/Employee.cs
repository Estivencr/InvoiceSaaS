using InvoiceSaaS.Domain.Interfaces;

namespace InvoiceSaaS.Domain.Entities;

public class Employee : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Position { get; set; }
    public Guid RoleId { get; set; }
    public DateTime? HireDate { get; set; }
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    public Company Company { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
