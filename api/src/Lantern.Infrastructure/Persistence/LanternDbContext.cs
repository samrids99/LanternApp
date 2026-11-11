using Lantern.Domain.Regions;
using Microsoft.EntityFrameworkCore;

namespace Lantern.Infrastructure.Persistence;

public class LanternDbContext : DbContext
{
  public LanternDbContext(DbContextOptions<LanternDbContext> options)
      : base(options) { }

  public DbSet<Region> Regions => Set<Region>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(LanternDbContext).Assembly);
  }
}
