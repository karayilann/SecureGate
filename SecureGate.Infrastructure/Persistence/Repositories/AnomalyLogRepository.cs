using Microsoft.EntityFrameworkCore;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Infrastructure.Persistence.Repositories;

public class AnomalyLogRepository : IAnomalyLogRepository
{
    private readonly AppDbContext _context;

    public AnomalyLogRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(AnomalyLog entity) => await _context.AnomalyLogs.AddAsync(entity);

    public async Task<AnomalyLog?> GetByIdAsync(Guid id) =>
        await _context.AnomalyLogs.FirstOrDefaultAsync(x => x.Id == id);

    public Task UpdateAsync(AnomalyLog entity)
    {
        _context.AnomalyLogs.Update(entity);
        return Task.CompletedTask;
    }
}
