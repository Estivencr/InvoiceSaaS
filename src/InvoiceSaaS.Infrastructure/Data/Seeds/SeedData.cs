using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InvoiceSaaS.Infrastructure.Data.Seeds;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InvoiceSaasDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<InvoiceSaasDbContext>>();

        try
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
}
