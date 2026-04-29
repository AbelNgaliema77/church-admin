using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Infrastructure.Persistence;
using ChurchAdmin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChurchAdmin.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ChurchAdminDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IChurchAdminDbContext>(provider =>
            provider.GetRequiredService<ChurchAdminDbContext>());

        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}