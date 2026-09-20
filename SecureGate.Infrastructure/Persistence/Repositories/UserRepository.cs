using Microsoft.EntityFrameworkCore;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Interfaces;

namespace SecureGate.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(User user) => await _context.Users.AddAsync(user);

    public async Task<User?> GetByIdAsync(Guid id) =>
        await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }
}
