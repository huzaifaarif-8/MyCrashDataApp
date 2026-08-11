using CrashDataApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CrashDataApp.Data;

public class CrashContext : DbContext
{
    public CrashContext(DbContextOptions<CrashContext> options) : base(options) { }

    public DbSet<Crash> Crashes => Set<Crash>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
            options.UseSqlite("Data Source=crashes.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Crash>(entity =>
        {
            entity.ToTable("Crashes");
            entity.HasIndex(c => c.Year);
            entity.HasIndex(c => c.Operator);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(u => u.Username).IsUnique();
        });
    }
}
