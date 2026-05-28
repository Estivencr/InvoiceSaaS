namespace InvoiceSaaS.Application.DTOs.Employee;

public class UpdateEmployeeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Position { get; set; }
    public Guid RoleId { get; set; }
    public DateTime? HireDate { get; set; }
    public string Status { get; set; } = "active";
}
