namespace InvoiceSaaS.Application.DTOs.Product;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public int Stock { get; set; }
    public string Unit { get; set; } = "unit";
    public bool IsActive { get; set; } = true;
}
