namespace InvoiceSaaS.Application.DTOs.Product;

public class ProductResponse
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public int Stock { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
