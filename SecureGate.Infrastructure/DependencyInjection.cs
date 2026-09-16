using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureGate.Application.Interfaces;
using SecureGate.Domain.Interfaces;
using SecureGate.Infrastructure.Persistence;
using SecureGate.Infrastructure.Persistence.Repositories;
using SecureGate.Infrastructure.RateLimiting;
using StackExchange.Redis;

namespace SecureGate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "SecureGate:";
        });

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

        services.AddSingleton<IRateLimiter, RedisRateLimiter>();

        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IUsageRepository, UsageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}