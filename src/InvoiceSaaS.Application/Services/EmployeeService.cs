using AutoMapper;
using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Employee;
using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSaaS.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public EmployeeService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoleResponse>> GetRolesAsync(Guid companyId, CancellationToken ct = default)
    {
        var roles = await _uow.Repository<Role>().FindAsync(r => r.CompanyId == companyId, ct);
        return roles.Select(r => new RoleResponse { Id = r.Id, Name = r.Name, Description = r.Description });
    }

    public async Task<PagedResult<EmployeeResponse>> GetAllAsync(Guid companyId, PaginationParams p, CancellationToken ct = default)
    {
        var query = _uow.Repository<Employee>().Query()
            .Include(e => e.Role)
            .Where(e => e.CompanyId == companyId && !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(p.Search))
            query = query.Where(e => e.Name.Contains(p.Search) || e.Email.Contains(p.Search));

        if (!string.IsNullOrWhiteSpace(p.Status))
            query = query.Where(e => e.Status == p.Status);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync(ct);

        return PagedResult<EmployeeResponse>.Create(_mapper.Map<IEnumerable<EmployeeResponse>>(items), total, p.Page, p.PageSize);
    }

    public async Task<EmployeeResponse> GetByIdAsync(Guid companyId, Guid id, CancellationToken ct = default)
    {
        var employee = await _uow.Repository<Employee>().Query()
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == companyId && !e.IsDeleted, ct)
            ?? throw new NotFoundException("Employee", id);

        return _mapper.Map<EmployeeResponse>(employee);
    }

    public async Task<IEnumerable<EmployeeResponse>> GetByRoleAsync(Guid companyId, Guid roleId, CancellationToken ct = default)
    {
        var employees = await _uow.Repository<Employee>().Query()
            .Include(e => e.Role)
            .Where(e => e.CompanyId == companyId && e.RoleId == roleId && !e.IsDeleted)
            .ToListAsync(ct);

        return _mapper.Map<IEnumerable<EmployeeResponse>>(employees);
    }

    public async Task<EmployeeResponse> CreateAsync(Guid companyId, CreateEmployeeRequest request, CancellationToken ct = default)
    {
        var emailExists = await _uow.Repository<Employee>()
            .FirstOrDefaultAsync(e => e.CompanyId == companyId && e.Email == request.Email && !e.IsDeleted, ct);
        if (emailExists != null)
            throw new BusinessException($"An employee with email '{request.Email}' already exists.");

        var role = await _uow.Repository<Role>()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId && r.CompanyId == companyId, ct)
            ?? throw new NotFoundException("Role", request.RoleId);

        var employee = _mapper.Map<Employee>(request);
        employee.Id = Guid.NewGuid();
        employee.CompanyId = companyId;

        await _uow.Repository<Employee>().AddAsync(employee, ct);
        await _uow.SaveChangesAsync(ct);

        employee.Role = role;
        return _mapper.Map<EmployeeResponse>(employee);
    }

    public async Task<EmployeeResponse> UpdateAsync(Guid companyId, Guid id, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        var employee = await _uow.Repository<Employee>().Query()
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == companyId && !e.IsDeleted, ct)
            ?? throw new NotFoundException("Employee", id);

        var emailTaken = await _uow.Repository<Employee>()
            .FirstOrDefaultAsync(e => e.CompanyId == companyId && e.Email == request.Email && e.Id != id && !e.IsDeleted, ct);
        if (emailTaken != null)
            throw new BusinessException("Email is already in use by another employee.");

        _mapper.Map(request, employee);
        employee.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<Employee>().Update(employee);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<EmployeeResponse>(employee);
    }

    public async Task DeleteAsync(Guid companyId, Guid id, CancellationToken ct = default)
    {
        var employee = await _uow.Repository<Employee>()
            .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == companyId && !e.IsDeleted, ct)
            ?? throw new NotFoundException("Employee", id);

        employee.IsDeleted = true;
        employee.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<Employee>().Update(employee);
        await _uow.SaveChangesAsync(ct);
    }
}
