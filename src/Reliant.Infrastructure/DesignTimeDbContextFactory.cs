using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Reliant.Infrastructure;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
  public AppDbContext CreateDbContext(string[] args)
  {
    // Prefer an override when you need it; otherwise use a sensible default.
    var cs =
      Environment.GetEnvironmentVariable("MIGRATIONS_CS")
      ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
      ?? "Host=localhost;Port=5432;Database=reliant;Username=postgres;Password=postgres;Include Error Detail=true";

    var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(cs).Options;

    return new AppDbContext(options);
  }
}
