using MediatR;
using SecureGate.Application.DTOs;

namespace SecureGate.Application.Features.Usage.Queries.GetUsageStats;

public record GetUsageStatsQuery : IRequest<UsageStatsDto>;
