using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Reliant.Application.DTOs;
using Reliant.Infrastructure;

namespace Reliant.Api.Endpoints;

public static class ProductEndpoints
{
  public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapGet(
        "/api/products",
        [Authorize(Policy = "SalesOrManager")]
        async (AppDbContext db) =>
        {
          var list = await db
            .Products.OrderBy(p => p.Name)
            .Select(p => new ProductResponse(p.Id, p.Name, p.ProductType, p.BasePrice))
            .ToListAsync();
          return Results.Ok(list);
        }
      )
      .WithTags("Products")
      .Produces<IEnumerable<ProductResponse>>(StatusCodes.Status200OK)
      .WithOpenApi(op => op.Summarize("List products", "Returns the product catalogue."));

    return app;
  }
}
