using Microsoft.EntityFrameworkCore;
using SecureGate.Domain.Entities;
using SecureGate.Domain.Enums;

namespace SecureGate.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();
        public DbSet<AnomalyLog> AnomalyLogs => Set<AnomalyLog>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Seed datas: Free / Pro / Enterprise plans
            modelBuilder.Entity<Plan>().HasData(
                new Plan { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = PlanType.Free, RequestsPerMinute = 10 },
                new Plan { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = PlanType.Pro, RequestsPerMinute = 100 },
                new Plan { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = PlanType.Enterprise, RequestsPerMinute = int.MaxValue }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Email = "admin@securegate.local",
                    PasswordHash = "3RXdI7PfZ/RUU/9EMnLAtg==.Xp4R9ifHr4ToJ2XGrP7Mix68tR6LNidF28Vt2oJOx1I=",
                    Role = UserRole.Admin,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }

    }
}
