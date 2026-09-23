using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureGate.Application.Interfaces;
using SecureGate.Domain.Interfaces;
using SecureGate.Infrastructure.Backend;
using SecureGate.Infrastructure.BackgroundJobs;
using SecureGate.Infrastructure.Caching;
using SecureGate.Infrastructure.Persistence;
using SecureGate.Infrastructure.Persistence.Repositories;
using SecureGate.Infrastructure.RateLimiting;
using SecureGate.Infrastructure.Security;
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

        services.AddSingleton<MockBackendService>();
        services.AddSingleton<IBackendService>(sp =>
            new ResilientBackendService(sp.GetRequiredService<MockBackendService>()));

        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddSingleton<IKeyedLock, KeyedLock>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUsageRepository, UsageRepository>();
        services.AddScoped<IAnomalyLogRepository, AnomalyLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<UsageQueue>();
        services.AddSingleton<IUsageQueue>(sp => sp.GetRequiredService<UsageQueue>());

        services.AddHostedService<AnomalyDetectionWorker>();
        services.AddHostedService<UsageFlushWorker>();

        return services;
    }
}