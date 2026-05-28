using InvoiceSaaS.Application.Services;
using InvoiceSaaS.Domain.Interfaces;
using InvoiceSaaS.Infrastructure.Data;
using InvoiceSaaS.Infrastructure.Identity;
using InvoiceSaaS.Infrastructure.Repositories;
using InvoiceSaaS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceSaaS.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<InvoiceSaasDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly("InvoiceSaaS.Infrastructure")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
