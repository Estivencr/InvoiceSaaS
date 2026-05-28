using AutoMapper;
using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Customer;
using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSaaS.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<CustomerResponse>> GetAllAsync(Guid companyId, PaginationParams p, CancellationToken ct = default)
    {
        var query = _uow.Repository<Customer>().Query()
            .Where(c => c.CompanyId == companyId && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(p.Search))
            query = query.Where(c => c.Name.Contains(p.Search) || c.Document.Contains(p.Search) || c.Email.Contains(p.Search));

        if (!string.IsNullOrWhiteSpace(p.Status))
            query = query.Where(c => c.Status == p.Status);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync(ct);

        return PagedResult<CustomerResponse>.Create(_mapper.Map<IEnumerable<CustomerResponse>>(items), total, p.Page, p.PageSize);
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid companyId, Guid id, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>()
            .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId && !c.IsDeleted, ct)
            ?? throw new NotFoundException("Customer", id);

        return _mapper.Map<CustomerResponse>(customer);
    }

    public async Task<CustomerResponse> CreateAsync(Guid companyId, Guid userId, CreateCustomerRequest request, CancellationToken ct = default)
    {
        var existing = await _uow.Repository<Customer>()
            .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Document == request.Document && !c.IsDeleted, ct);

        if (existing != null)
            throw new BusinessException($"A customer with document '{request.Document}' already exists.");

        var customer = _mapper.Map<Customer>(request);
        customer.Id = Guid.NewGuid();
        customer.CompanyId = companyId;
        customer.CreatedBy = userId;

        await _uow.Repository<Customer>().AddAsync(customer, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<CustomerResponse>(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid companyId, Guid id, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>()
            .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId && !c.IsDeleted, ct)
            ?? throw new NotFoundException("Customer", id);

        var docTaken = await _uow.Repository<Customer>()
            .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Document == request.Document && c.Id != id && !c.IsDeleted, ct);

        if (docTaken != null)
            throw new BusinessException($"Document '{request.Document}' is already used by another customer.");

        _mapper.Map(request, customer);
        customer.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<Customer>().Update(customer);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<CustomerResponse>(customer);
    }

    public async Task DeleteAsync(Guid companyId, Guid id, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>()
            .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId && !c.IsDeleted, ct)
            ?? throw new NotFoundException("Customer", id);

        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<Customer>().Update(customer);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<CustomerResponse>> SearchAsync(Guid companyId, string term, CancellationToken ct = default)
    {
        var customers = await _uow.Repository<Customer>()
            .FindAsync(c => c.CompanyId == companyId && !c.IsDeleted &&
                (c.Name.Contains(term) || c.Document.Contains(term) || c.Email.Contains(term)), ct);

        return _mapper.Map<IEnumerable<CustomerResponse>>(customers);
    }
}
