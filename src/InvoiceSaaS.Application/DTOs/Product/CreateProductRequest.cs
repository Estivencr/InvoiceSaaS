namespace InvoiceSaaS.Application.DTOs.Product;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public int Stock { get; set; } = 0;
    public string Unit { get; set; } = "unit";
}
