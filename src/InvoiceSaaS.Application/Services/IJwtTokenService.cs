using InvoiceSaaS.Domain.Entities;

namespace InvoiceSaaS.Application.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiry();
}
