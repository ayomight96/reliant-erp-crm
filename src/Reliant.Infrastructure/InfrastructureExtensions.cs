using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reliant.Infrastructure;

public static class InfrastructureExtensions
{
  public static IServiceCollection AddAppDb(this IServiceCollection services, IConfiguration config)
  {
    var cs = config.GetConnectionString("Default");
    // If we got a Postgres connection (contains Host=...), use Npgsql; else default to SQLite
    if (!string.IsNullOrWhiteSpace(cs) && cs.Contains("Host=", StringComparison.OrdinalIgnoreCase))
    {
      services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(cs));
    }
    else
    {
      services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=app.db"));
    }
    return services;
  }
}
