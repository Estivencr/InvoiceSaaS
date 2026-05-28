using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Invoice;
using InvoiceSaaS.Application.Mappings;
using InvoiceSaaS.Application.Services;
using InvoiceSaaS.Application.Tests.Fixtures;
using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Enums;
using InvoiceSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace InvoiceSaaS.Application.Tests.Services;

public class InvoiceServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IRepository<Invoice>> _invoiceRepoMock = new();
    private readonly Mock<IRepository<Customer>> _customerRepoMock = new();
    private readonly Mock<ILogger<InvoiceService>> _loggerMock = new();
    private readonly IMapper _mapper;

    public InvoiceServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
        _uowMock.Setup(u => u.Repository<Invoice>()).Returns(_invoiceRepoMock.Object);
        _uowMock.Setup(u => u.Repository<Customer>()).Returns(_customerRepoMock.Object);
    }

    private InvoiceService CreateService() => new(_uowMock.Object, _mapper, _loggerMock.Object);

    [Fact]
    public async Task UpdateStatusAsync_PendingToPaid_Succeeds()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var invoice = TestDataBuilder.BuildInvoice(companyId, customerId, userId, InvoiceStatus.Pending);
        invoice.Customer = TestDataBuilder.BuildCustomer(companyId);
        invoice.Customer.Id = customerId;
        invoice.CreatedBy = TestDataBuilder.BuildUser(companyId);

        _invoiceRepoMock
            .Setup(r => r.Query())
            .Returns(new List<Invoice> { invoice }.AsAsyncQueryable());

        var service = CreateService();
        var request = new UpdateInvoiceStatusRequest { Status = InvoiceStatus.Paid };

        // Act
        var result = await service.UpdateStatusAsync(companyId, invoice.Id, request);

        // Assert
        result.Should().NotBeNull();
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_PaidToPending_ThrowsBusinessException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var invoice = TestDataBuilder.BuildInvoice(companyId, customerId, userId, InvoiceStatus.Paid);
        invoice.Customer = TestDataBuilder.BuildCustomer(companyId);
        invoice.CreatedBy = TestDataBuilder.BuildUser(companyId);

        _invoiceRepoMock
            .Setup(r => r.Query())
            .Returns(new List<Invoice> { invoice }.AsAsyncQueryable());

        var service = CreateService();
        var request = new UpdateInvoiceStatusRequest { Status = InvoiceStatus.Pending };

        // Act
        var act = async () => await service.UpdateStatusAsync(companyId, invoice.Id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*Invalid status transition*");
    }

    [Fact]
    public void Invoice_RecalculateTotals_CalculatesCorrectly()
    {
        // Arrange
        var invoice = new Invoice { TaxRate = 19m };
        var detail = new InvoiceDetail { Quantity = 5, UnitPrice = 100_000m };
        detail.CalculateAmount();
        invoice.Details.Add(detail);

        // Act
        invoice.RecalculateTotals();

        // Assert
        invoice.Subtotal.Should().Be(500_000m);
        invoice.TaxAmount.Should().Be(95_000m);
        invoice.Total.Should().Be(595_000m);
    }
}
