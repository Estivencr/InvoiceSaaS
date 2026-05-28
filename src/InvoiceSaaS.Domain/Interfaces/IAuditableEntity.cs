namespace InvoiceSaaS.Domain.Interfaces;

public interface IAuditableEntity : IEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
    bool IsDeleted { get; set; }
}
