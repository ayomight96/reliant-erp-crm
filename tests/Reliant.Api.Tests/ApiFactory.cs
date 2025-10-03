using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reliant.Api.Integration.AiPricing;
using Reliant.Infrastructure;

namespace Reliant.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
  private SqliteConnection? _conn;

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");

    builder.ConfigureServices(services =>
    {
      // Replace AppDbContext options with in-memory SQLite
      services.RemoveAll<DbContextOptions<AppDbContext>>();

      _conn = new SqliteConnection("DataSource=:memory:");
      _conn.Open();

      services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conn));

      // Replace IAiPricingClient with a deterministic fake
      services.RemoveAll<IAiPricingClient>();
      services.AddSingleton<IAiPricingClient, FakeAiPricingClient>();

      // Build provider and create schema (no migrations in tests)
      var sp = services.BuildServiceProvider();
      using var scope = sp.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      db.Database.EnsureCreated();
      // NOTE: Do not seed here; Program.cs seeds for Testing env
    });
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    _conn?.Dispose();
    _conn = null;
  }
}

// Deterministic AI stub for tests
file sealed class FakeAiPricingClient : IAiPricingClient
{
  public Task<IReadOnlyList<decimal>> PredictUnitPricesAsync(
    IEnumerable<AiQuoteItemIn> items,
    CancellationToken ct = default
  ) => Task.FromResult<IReadOnlyList<decimal>>(items.Select(_ => 321.23m).ToList());
}
