using Microsoft.EntityFrameworkCore;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Enums;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Infrastructure.Persistence.Repositories
{
    public class PlanRepository : IPlanRepository
    {
        private readonly AppDbContext _context;

        public PlanRepository(AppDbContext context) => _context = context;

        public async Task<Plan?> GetByTypeAsync(PlanType planType)
        {
            return await _context.Plans.FirstOrDefaultAsync(p => p.Name == planType);
        }
    }
}
