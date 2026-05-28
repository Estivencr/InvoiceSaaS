using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Linq.Expressions;
using System.Text;
using Xunit;
using InvoiceSaaS.Application.Mappings;
using InvoiceSaaS.Application.Services;
using InvoiceSaaS.Application.Tests.Fixtures;
using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Enums;
using InvoiceSaaS.Domain.Interfaces;

namespace InvoiceSaaS.Application.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IRepository<Invoice>> _invoiceRepoMock = new();
    private readonly Mock<IRepository<Customer>> _customerRepoMock = new();
    private readonly IMapper _mapper;

    public ReportServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
        _uowMock.Setup(u => u.Repository<Invoice>()).Returns(_invoiceRepoMock.Object);
        _uowMock.Setup(u => u.Repository<Customer>()).Returns(_customerRepoMock.Object);
    }

    private ReportService CreateService() => new(_uowMock.Object, _mapper);

    private Invoice BuildInvoiceWithNav(Guid companyId, Guid customerId, Guid userId,
        InvoiceStatus status, DateTime? issueDate = null)
    {
        var customer = TestDataBuilder.BuildCustomer(companyId);
        customer.Id = customerId;

        var user = TestDataBuilder.BuildUser(companyId);
        user.Id = userId;

        var invoice = TestDataBuilder.BuildInvoice(companyId, customerId, userId, status);
        invoice.Customer = customer;
        invoice.CreatedBy = user;

        if (issueDate.HasValue)
            invoice.IssueDate = issueDate.Value;

        return invoice;
    }

    // ── Dashboard ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDashboardAsync_ReturnsCorrectTotals()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var paid = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Paid);
        var pending = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Pending);
        var cancelled = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Cancelled);

        _invoiceRepoMock.Setup(r => r.Query())
            .Returns(new List<Invoice> { paid, pending, cancelled }.AsAsyncQueryable());
        _customerRepoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>(), default))
            .ReturnsAsync(5);

        // Act
        var result = await CreateService().GetDashboardAsync(companyId);

        // Assert
        result.TotalInvoices.Should().Be(3);
        result.TotalRevenue.Should().Be(paid.Total);
        result.PendingAmount.Should().Be(pending.Total);
        result.ActiveCustomers.Should().Be(5);
        result.ThisMonthRevenue.Should().Be(paid.Total);
        result.InvoicesByStatus.Paid.Should().Be(1);
        result.InvoicesByStatus.Pending.Should().Be(1);
        result.InvoicesByStatus.Cancelled.Should().Be(1);
        result.RecentInvoices.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetDashboardAsync_EmptyCompany_ReturnsZeroes()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _invoiceRepoMock.Setup(r => r.Query()).Returns(new List<Invoice>().AsAsyncQueryable());
        _customerRepoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>(), default))
            .ReturnsAsync(0);

        // Act
        var result = await CreateService().GetDashboardAsync(companyId);

        // Assert
        result.TotalInvoices.Should().Be(0);
        result.TotalRevenue.Should().Be(0m);
        result.PendingAmount.Should().Be(0m);
        result.ActiveCustomers.Should().Be(0);
        result.RecentInvoices.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboardAsync_RecentInvoices_LimitedToTen()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var invoices = Enumerable.Range(0, 15)
            .Select(_ => BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Paid))
            .ToList();

        _invoiceRepoMock.Setup(r => r.Query()).Returns(invoices.AsAsyncQueryable());
        _customerRepoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>(), default))
            .ReturnsAsync(1);

        // Act
        var result = await CreateService().GetDashboardAsync(companyId);

        // Assert
        result.RecentInvoices.Should().HaveCount(10);
    }

    // ── Monthly Sales ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMonthlySalesAsync_GroupsCorrectlyByMonth()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var jan1 = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Paid,
            new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc));
        var jan2 = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Pending,
            new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc));
        var feb = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Paid,
            new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc));

        _invoiceRepoMock.Setup(r => r.Query())
            .Returns(new List<Invoice> { jan1, jan2, feb }.AsAsyncQueryable());

        // Act
        var result = (await CreateService().GetMonthlySalesAsync(companyId, months: 12)).ToList();

        // Assert
        result.Should().HaveCount(2);

        var january = result.Single(r => r.Month == 1 && r.Year == 2026);
        january.InvoiceCount.Should().Be(2);
        january.TotalRevenue.Should().Be(jan1.Total + jan2.Total);
        january.PaidAmount.Should().Be(jan1.Total);
        january.PendingAmount.Should().Be(jan2.Total);
        january.MonthName.Should().NotBeNullOrEmpty();

        var february = result.Single(r => r.Month == 2 && r.Year == 2026);
        february.InvoiceCount.Should().Be(1);
        february.PaidAmount.Should().Be(feb.Total);
    }

    [Fact]
    public async Task GetMonthlySalesAsync_OrderedByYearThenMonth()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var march = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Paid,
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        var january = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Paid,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        _invoiceRepoMock.Setup(r => r.Query())
            .Returns(new List<Invoice> { march, january }.AsAsyncQueryable());

        // Act
        var result = (await CreateService().GetMonthlySalesAsync(companyId)).ToList();

        // Assert
        result[0].Month.Should().Be(1);
        result[1].Month.Should().Be(3);
    }

    // ── Top Customers ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTopCustomersAsync_SortsByTotalAmountDescending()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();

        var inv1 = BuildInvoiceWithNav(companyId, customer1Id, userId, InvoiceStatus.Paid);
        var inv2 = BuildInvoiceWithNav(companyId, customer2Id, userId, InvoiceStatus.Paid);
        var inv3 = BuildInvoiceWithNav(companyId, customer2Id, userId, InvoiceStatus.Paid);

        _invoiceRepoMock.Setup(r => r.Query())
            .Returns(new List<Invoice> { inv1, inv2, inv3 }.AsAsyncQueryable());

        // Act
        var result = (await CreateService().GetTopCustomersAsync(companyId, top: 10)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].CustomerId.Should().Be(customer2Id);
        result[0].TotalAmount.Should().Be(inv2.Total + inv3.Total);
        result[0].InvoiceCount.Should().Be(2);
        result[1].CustomerId.Should().Be(customer1Id);
        result[1].InvoiceCount.Should().Be(1);
    }

    [Fact]
    public async Task GetTopCustomersAsync_RespectsTopLimit()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var invoices = Enumerable.Range(0, 8)
            .Select(_ => BuildInvoiceWithNav(companyId, Guid.NewGuid(), userId, InvoiceStatus.Paid))
            .ToList();

        _invoiceRepoMock.Setup(r => r.Query()).Returns(invoices.AsAsyncQueryable());

        // Act
        var result = await CreateService().GetTopCustomersAsync(companyId, top: 3);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetTopCustomersAsync_ExcludesNonPaidInvoices()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var paid = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Paid);
        var pending = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Pending);
        var cancelled = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Cancelled);

        _invoiceRepoMock.Setup(r => r.Query())
            .Returns(new List<Invoice> { paid, pending, cancelled }.AsAsyncQueryable());

        // Act
        var result = (await CreateService().GetTopCustomersAsync(companyId)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].InvoiceCount.Should().Be(1);
        result[0].TotalAmount.Should().Be(paid.Total);
    }

    // ── Revenue by Period ────────────────────────────────────────────────────

    [Fact]
    public async Task GetRevenueByPeriodAsync_FiltersByDateRangeAndAggregatesByStatus()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var paid = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Paid,
            new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc));
        var pending = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Pending,
            new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc));
        var cancelled = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Cancelled,
            new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc));
        var outOfRange = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Paid,
            new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));

        _invoiceRepoMock.Setup(r => r.Query())
            .Returns(new List<Invoice> { paid, pending, cancelled, outOfRange }.AsAsyncQueryable());

        var from = new DateTime(2026, 3, 1);
        var to = new DateTime(2026, 3, 31);

        // Act
        var result = await CreateService().GetRevenueByPeriodAsync(companyId, from, to);

        // Assert
        result.TotalInvoices.Should().Be(3);
        result.PaidInvoices.Should().Be(1);
        result.PendingInvoices.Should().Be(1);
        result.CancelledInvoices.Should().Be(1);
        result.PaidRevenue.Should().Be(paid.Total);
        result.PendingRevenue.Should().Be(pending.Total);
        result.TotalRevenue.Should().Be(paid.Total + pending.Total + cancelled.Total);
        result.DateFrom.Should().Be(from);
        result.DateTo.Should().Be(to);
    }

    [Fact]
    public async Task GetRevenueByPeriodAsync_EmptyRange_ReturnsZeroes()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _invoiceRepoMock.Setup(r => r.Query()).Returns(new List<Invoice>().AsAsyncQueryable());

        // Act
        var result = await CreateService().GetRevenueByPeriodAsync(
            companyId,
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31));

        // Assert
        result.TotalInvoices.Should().Be(0);
        result.TotalRevenue.Should().Be(0m);
    }

    // ── CSV Export ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ExportInvoicesToCsvAsync_ContainsCsvHeaderAndInvoiceData()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var invoice = BuildInvoiceWithNav(companyId, Guid.NewGuid(), Guid.NewGuid(), InvoiceStatus.Paid);

        _invoiceRepoMock.Setup(r => r.Query())
            .Returns(new List<Invoice> { invoice }.AsAsyncQueryable());

        // Act
        var csvBytes = await CreateService().ExportInvoicesToCsvAsync(companyId, null, null);
        var csv = Encoding.UTF8.GetString(csvBytes);

        // Assert
        csv.Should().StartWith("InvoiceNumber,Customer,Document,IssueDate,DueDate,Subtotal,TaxRate,TaxAmount,Total,Status,PaymentDate");
        csv.Should().Contain(invoice.InvoiceNumber);
        csv.Should().Contain(invoice.Customer.Name);
        csv.Should().Contain(invoice.Customer.Document);
    }

    [Fact]
    public async Task ExportInvoicesToCsvAsync_WithDateFilter_ExcludesOutOfRangeInvoices()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var inRange = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Paid,
            new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc));
        inRange.InvoiceNumber = "INV-2026-04-00001";

        var tooEarly = BuildInvoiceWithNav(companyId, customerId, userId, InvoiceStatus.Paid,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        tooEarly.InvoiceNumber = "INV-2026-01-00001";

        _invoiceRepoMock.Setup(r => r.Query())
            .Returns(new List<Invoice> { inRange, tooEarly }.AsAsyncQueryable());

        // Act
        var csvBytes = await CreateService().ExportInvoicesToCsvAsync(
            companyId,
            dateFrom: new DateTime(2026, 4, 1),
            dateTo: new DateTime(2026, 4, 30));
        var csv = Encoding.UTF8.GetString(csvBytes);

        // Assert
        csv.Should().Contain("INV-2026-04-00001");
        csv.Should().NotContain("INV-2026-01-00001");
    }

    [Fact]
    public async Task ExportInvoicesToCsvAsync_CustomerNameWithComma_EscapedCorrectly()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var invoice = BuildInvoiceWithNav(companyId, Guid.NewGuid(), Guid.NewGuid(), InvoiceStatus.Paid);
        invoice.Customer.Name = "Empresa, S.A.";

        _invoiceRepoMock.Setup(r => r.Query())
            .Returns(new List<Invoice> { invoice }.AsAsyncQueryable());

        // Act
        var csv = Encoding.UTF8.GetString(
            await CreateService().ExportInvoicesToCsvAsync(companyId, null, null));

        // Assert
        csv.Should().Contain("\"Empresa, S.A.\"");
    }
}
