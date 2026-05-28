using AutoMapper;
using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.User;
using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSaaS.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserResponse>> GetAllAsync(Guid companyId, PaginationParams p, CancellationToken ct = default)
    {
        var query = _uow.Repository<User>().Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Where(u => u.CompanyId == companyId && !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(p.Search))
            query = query.Where(u => u.Email.Contains(p.Search) || (u.FirstName + " " + u.LastName).Contains(p.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync(ct);

        return PagedResult<UserResponse>.Create(_mapper.Map<IEnumerable<UserResponse>>(items), total, p.Page, p.PageSize);
    }

    public async Task<UserResponse> GetByIdAsync(Guid companyId, Guid id, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId && !u.IsDeleted, ct)
            ?? throw new NotFoundException("User", id);

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> CreateAsync(Guid companyId, CreateUserRequest request, CancellationToken ct = default)
    {
        var emailExists = await _uow.Repository<User>()
            .FirstOrDefaultAsync(u => u.CompanyId == companyId && u.Email == request.Email && !u.IsDeleted, ct);
        if (emailExists != null)
            throw new BusinessException($"Email '{request.Email}' is already registered in this company.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 10)
        };

        await _uow.Repository<User>().AddAsync(user, ct);

        foreach (var roleId in request.RoleIds)
        {
            var role = await _uow.Repository<Role>()
                .FirstOrDefaultAsync(r => r.Id == roleId && r.CompanyId == companyId, ct)
                ?? throw new NotFoundException("Role", roleId);

            await _uow.Repository<UserRole>().AddAsync(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = role.Id
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(companyId, user.Id, ct);
    }

    public async Task<UserResponse> UpdateAsync(Guid companyId, Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>()
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId && !u.IsDeleted, ct)
            ?? throw new NotFoundException("User", id);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _uow.Repository<User>().Update(user);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(companyId, id, ct);
    }

    public async Task DeleteAsync(Guid companyId, Guid id, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>()
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId && !u.IsDeleted, ct)
            ?? throw new NotFoundException("User", id);

        user.IsDeleted = true;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<User>().Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<UserResponse> ChangeRoleAsync(Guid companyId, Guid id, ChangeRoleRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId && !u.IsDeleted, ct)
            ?? throw new NotFoundException("User", id);

        foreach (var ur in user.UserRoles.ToList())
            _uow.Repository<UserRole>().Remove(ur);

        foreach (var roleId in request.RoleIds)
        {
            var role = await _uow.Repository<Role>()
                .FirstOrDefaultAsync(r => r.Id == roleId && r.CompanyId == companyId, ct)
                ?? throw new NotFoundException("Role", roleId);

            await _uow.Repository<UserRole>().AddAsync(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = role.Id
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(companyId, id, ct);
    }

    public async Task<UserResponse> ToggleStatusAsync(Guid companyId, Guid id, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>()
            .FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId && !u.IsDeleted, ct)
            ?? throw new NotFoundException("User", id);

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<User>().Update(user);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(companyId, id, ct);
    }
}
