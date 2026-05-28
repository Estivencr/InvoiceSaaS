using InvoiceSaaS.Domain.Interfaces;

namespace InvoiceSaaS.Domain.Entities;

public class AuditLog : IEntity
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid? UserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }

    public Company Company { get; set; } = null!;
    public User? User { get; set; }
}
