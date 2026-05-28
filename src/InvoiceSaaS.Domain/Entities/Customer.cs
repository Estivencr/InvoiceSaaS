using InvoiceSaaS.Domain.Interfaces;

namespace InvoiceSaaS.Domain.Entities;

public class Customer : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string Status { get; set; } = "active";
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    public Company Company { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
