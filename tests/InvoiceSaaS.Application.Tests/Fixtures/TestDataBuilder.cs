using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Enums;

namespace InvoiceSaaS.Application.Tests.Fixtures;

public static class TestDataBuilder
{
    public static Company BuildCompany(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = "Test Company S.A.S",
        Email = "test@company.com",
        Country = "Colombia",
        IsActive = true
    };

    public static Role BuildRole(Guid companyId, string name = "Admin") => new()
    {
        Id = Guid.NewGuid(),
        CompanyId = companyId,
        Name = name,
        Description = $"{name} role"
    };

    public static User BuildUser(Guid companyId, string? passwordHash = null) => new()
    {
        Id = Guid.NewGuid(),
        CompanyId = companyId,
        Email = "admin@company.com",
        FirstName = "John",
        LastName = "Doe",
        PasswordHash = passwordHash ?? BCrypt.Net.BCrypt.HashPassword("Admin123!"),
        IsActive = true
    };

    public static Customer BuildCustomer(Guid companyId) => new()
    {
        Id = Guid.NewGuid(),
        CompanyId = companyId,
        Name = "Acme Corp",
        Document = "900123456",
        Email = "billing@acme.com",
        Status = "active"
    };

    public static Invoice BuildInvoice(Guid companyId, Guid customerId, Guid userId, InvoiceStatus status = InvoiceStatus.Pending)
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            InvoiceNumber = "INV-2024-01-00001",
            CustomerId = customerId,
            CreatedById = userId,
            IssueDate = DateTime.UtcNow,
            TaxRate = 19m,
            Status = status
        };

        var detail = new InvoiceDetail
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            Description = "Software development services",
            Quantity = 10,
            UnitPrice = 100_000m,
            Sequence = 1
        };
        detail.CalculateAmount();
        invoice.Details.Add(detail);
        invoice.RecalculateTotals();

        return invoice;
    }
}
