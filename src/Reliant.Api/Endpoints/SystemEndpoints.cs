using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Reliant.Infrastructure;

namespace Reliant.Api.Endpoints;

public static class SystemEndpoints
{
  public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapGet("/healthz", () => Results.Ok(new { status = "ok", service = "reliant-api" }))
      .WithTags("Health")
      .WithOpenApi(op => op.Summarize("Health check", "Liveness probe for the API."));

    app.MapGet(
        "/debug/counts",
        async (AppDbContext db) =>
        {
          var counts = new
          {
            users = await db.Users.CountAsync(),
            roles = await db.Roles.CountAsync(),
            customers = await db.Customers.CountAsync(),
            products = await db.Products.CountAsync(),
            quotes = await db.Quotes.CountAsync(),
            items = await db.QuoteItems.CountAsync(),
          };
          return Results.Ok(counts);
        }
      )
      .WithTags("Debug")
      .Produces(StatusCodes.Status200OK)
      .WithOpenApi(op => op.Summarize("Entity counts", "Quick row counts for main tables."));

    return app;
  }
}
