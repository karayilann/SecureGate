using Microsoft.EntityFrameworkCore;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Infrastructure.Persistence.Repositories
{
    public class UsageRepository : IUsageRepository
    {
        private readonly AppDbContext _context;

        public UsageRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(UsageRecord usage) => await _context.UsageRecords.AddAsync(usage);

        public async Task<UsageRecord?> GetByIdAsync(Guid id) =>
            await _context.UsageRecords.FirstOrDefaultAsync(x => x.Id == id);

        public async Task<List<UsageRecord>> GetRecentByApiKeyIdAsync(Guid apiKeyId, DateTime since)
            => await _context.UsageRecords.AsNoTracking().Where(u => u.ApiKeyId == apiKeyId && u.Timestamp >= since).ToListAsync();

        public async Task<List<UsageRecord>> GetRecentAsync(DateTime since)
            => await _context.UsageRecords.AsNoTracking().Where(u => u.Timestamp >= since).ToListAsync();

        public async Task UpdateAsync(UsageRecord entity)
        {
            _context.UsageRecords.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}