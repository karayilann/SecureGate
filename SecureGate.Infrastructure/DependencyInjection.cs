using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureGate.Domain.Interfaces;
using SecureGate.Infrastructure.Persistence;
using SecureGate.Infrastructure.Persistence.Repositories;

namespace SecureGate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IUsageRepository, UsageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}