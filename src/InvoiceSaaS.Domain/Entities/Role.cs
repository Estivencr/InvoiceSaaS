using InvoiceSaaS.Domain.Interfaces;

namespace InvoiceSaaS.Domain.Entities;

public class Role : IEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCustom { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Company Company { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
