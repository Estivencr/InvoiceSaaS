namespace InvoiceSaaS.Application.DTOs.Auth;

public class RegisterRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string? CompanyPhone { get; set; }
    public string? CompanyDocument { get; set; }
    public string? CompanyCountry { get; set; }

    public string AdminFirstName { get; set; } = string.Empty;
    public string AdminLastName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
