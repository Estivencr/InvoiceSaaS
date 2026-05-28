using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Customer;
using InvoiceSaaS.Application.Mappings;
using InvoiceSaaS.Application.Services;
using InvoiceSaaS.Application.Tests.Fixtures;
using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Interfaces;
using Moq;

namespace InvoiceSaaS.Application.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IRepository<Customer>> _customerRepoMock = new();
    private readonly IMapper _mapper;

    public CustomerServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
        _uowMock.Setup(u => u.Repository<Customer>()).Returns(_customerRepoMock.Object);
    }

    private CustomerService CreateService() => new(_uowMock.Object, _mapper);

    [Fact]
    public async Task GetByIdAsync_ExistingCustomer_ReturnsCustomerResponse()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customer = TestDataBuilder.BuildCustomer(companyId);

        _customerRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>(), default))
            .ReturnsAsync(customer);

        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(companyId, customer.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customer.Id);
        result.Name.Should().Be(customer.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _customerRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>(), default))
            .ReturnsAsync((Customer?)null);

        var service = CreateService();

        // Act
        var act = async () => await service.GetByIdAsync(companyId, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_DuplicateDocument_ThrowsBusinessException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var existing = TestDataBuilder.BuildCustomer(companyId);

        _customerRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>(), default))
            .ReturnsAsync(existing);

        var service = CreateService();
        var request = new CreateCustomerRequest { Document = existing.Document, Name = "Other", Email = "other@test.com" };

        // Act
        var act = async () => await service.CreateAsync(companyId, Guid.NewGuid(), request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().WithMessage("*document*");
    }

    [Fact]
    public async Task DeleteAsync_ExistingCustomer_SetsIsDeleted()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var customer = TestDataBuilder.BuildCustomer(companyId);

        _customerRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>(), default))
            .ReturnsAsync(customer);

        var service = CreateService();

        // Act
        await service.DeleteAsync(companyId, customer.Id);

        // Assert
        customer.IsDeleted.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
