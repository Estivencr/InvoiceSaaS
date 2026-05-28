using InvoiceSaaS.Domain.Interfaces;

namespace InvoiceSaaS.Domain.Entities;

public class Company : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Document { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string SubscriptionPlan { get; set; } = "free";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
