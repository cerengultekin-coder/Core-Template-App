using CoreApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreApp.Infrastructure.Data;

public class CoreAppDbContext : DbContext
{
    public CoreAppDbContext(DbContextOptions<CoreAppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreAppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
