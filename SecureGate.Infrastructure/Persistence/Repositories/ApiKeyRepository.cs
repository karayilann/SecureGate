
using Microsoft.EntityFrameworkCore;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Infrastructure.Persistence.Repositories
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly AppDbContext _context;

        public ApiKeyRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(ApiKey apiKey) => await _context.ApiKeys.AddAsync(apiKey);

        public async Task<ApiKey?> GetByIdAsync(Guid id) => await _context.ApiKeys.Include(x => x.Plan).FirstOrDefaultAsync(x => x.Id == id);

        public async Task<ApiKey?> GetByKeyValueAsync(string keyValue) => await _context.ApiKeys.Include(x => x.Plan).FirstOrDefaultAsync(x => x.KeyValue == keyValue);

        public async Task<List<ApiKey>> GetAllAsync() =>
            await _context.ApiKeys.AsNoTracking().Include(x => x.Plan).OrderByDescending(x => x.CreatedAt).ToListAsync();

        public Task UpdateAsync(ApiKey apiKey)
        {
            _context.ApiKeys.Update(apiKey);
            return Task.CompletedTask;
        }
    }
}
