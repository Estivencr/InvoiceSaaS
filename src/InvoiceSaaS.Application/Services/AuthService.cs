using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Auth;
using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceSaaS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtTokenService _jwt;
    private readonly ILogger<AuthService> _logger;

    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    public AuthService(IUnitOfWork uow, IJwtTokenService jwt, ILogger<AuthService> logger)
    {
        _uow = uow;
        _jwt = jwt;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, ct)
            ?? throw new UnauthorizedException("Invalid credentials.");

        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            throw new BusinessException($"Account locked. Try again after {user.LockedUntil:HH:mm} UTC.", 429);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                _logger.LogWarning("Account locked for user {Email} after {Attempts} failed attempts.", user.Email, user.FailedLoginAttempts);
            }
            _uow.Repository<User>().Update(user);
            await _uow.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (!user.IsActive)
            throw new UnauthorizedException("Account is inactive.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _jwt.GenerateAccessToken(user, roles);
        var refreshToken = _jwt.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = _jwt.GetRefreshTokenExpiry();
        user.LastLogin = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        _uow.Repository<User>().Update(user);
        await _uow.SaveChangesAsync(ct);

        return BuildLoginResponse(user, accessToken, refreshToken, roles);
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var emailExists = await _uow.Repository<Company>()
            .FirstOrDefaultAsync(c => c.Email == request.CompanyEmail, ct);
        if (emailExists != null)
            throw new BusinessException("A company with this email already exists.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = request.CompanyName,
                Email = request.CompanyEmail,
                Phone = request.CompanyPhone,
                Document = request.CompanyDocument,
                Country = request.CompanyCountry
            };
            await _uow.Repository<Company>().AddAsync(company, ct);

            var adminRole = new Role
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                Name = "Admin",
                Description = "Company administrator"
            };
            var managerRole = new Role { Id = Guid.NewGuid(), CompanyId = company.Id, Name = "Manager" };
            var employeeRole = new Role { Id = Guid.NewGuid(), CompanyId = company.Id, Name = "Employee" };

            await _uow.Repository<Role>().AddAsync(adminRole, ct);
            await _uow.Repository<Role>().AddAsync(managerRole, ct);
            await _uow.Repository<Role>().AddAsync(employeeRole, ct);

            var user = new User
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                Email = request.AdminEmail,
                FirstName = request.AdminFirstName,
                LastName = request.AdminLastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword, 10)
            };
            await _uow.Repository<User>().AddAsync(user, ct);

            var userRole = new UserRole { Id = Guid.NewGuid(), UserId = user.Id, RoleId = adminRole.Id };
            await _uow.Repository<UserRole>().AddAsync(userRole, ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            var accessToken = _jwt.GenerateAccessToken(user, new[] { "Admin" });
            var refreshToken = _jwt.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = _jwt.GetRefreshTokenExpiry();
            user.LastLogin = DateTime.UtcNow;
            _uow.Repository<User>().Update(user);
            await _uow.SaveChangesAsync(ct);

            return BuildLoginResponse(user, accessToken, refreshToken, new[] { "Admin" });
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && !u.IsDeleted, ct)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token expired.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _jwt.GenerateAccessToken(user, roles);
        var refreshToken = _jwt.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = _jwt.GetRefreshTokenExpiry();
        _uow.Repository<User>().Update(user);
        await _uow.SaveChangesAsync(ct);

        return BuildLoginResponse(user, accessToken, refreshToken, roles);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        _uow.Repository<User>().Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            throw new BusinessException("Passwords do not match.");

        var user = await _uow.Repository<User>().GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new BusinessException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 10);
        user.UpdatedAt = DateTime.UtcNow;
        user.RefreshToken = null;
        _uow.Repository<User>().Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    private static LoginResponse BuildLoginResponse(User user, string accessToken, string refreshToken, IEnumerable<string> roles) =>
        new()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CompanyId = user.CompanyId,
                Roles = roles.ToList()
            }
        };
}
