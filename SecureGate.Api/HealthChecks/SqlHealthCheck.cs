using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SecureGate.Infrastructure.Persistence;

namespace SecureGate.Api.HealthChecks;

public sealed class SqlHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public SqlHealthCheck(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("SQL Server reachable.")
            : HealthCheckResult.Unhealthy("SQL Server unreachable.");
    }
}
