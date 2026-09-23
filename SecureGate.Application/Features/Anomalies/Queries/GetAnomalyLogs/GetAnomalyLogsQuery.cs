using MediatR;
using SecureGate.Application.DTOs;

namespace SecureGate.Application.Features.Anomalies.Queries.GetAnomalyLogs;

public record GetAnomalyLogsQuery : IRequest<IReadOnlyList<AnomalyLogDto>>;
