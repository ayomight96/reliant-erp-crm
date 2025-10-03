using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reliant.Api.Endpoints;
using Reliant.Api.Integration.AiPricing;
using Reliant.Application.DTOs;
using Reliant.Application.Services;
using Reliant.Application.Validation;
using Reliant.Domain.Entities;
using Reliant.Infrastructure;

namespace Reliant.Api.Endpoints;

public static class QuoteEndpoints
{
  public static IEndpointRouteBuilder MapQuoteEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api").WithTags("Quotes").RequireAuthorization("SalesOrManager");

    group
      .MapGet(
        "/customers/{customerId:int}/quotes",
        async (int customerId, AppDbContext db) =>
        {
          var quotes = await db
            .Quotes.Include(q => q.Items)
            .ThenInclude(i => i.Product)
            .Include(q => q.Customer)
            .Where(q => q.CustomerId == customerId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

          var result = quotes.Select(q => new QuoteResponse(
            q.Id,
            q.CustomerId,
            q.Customer.Name,
            q.Status,
            q.Subtotal,
            q.Vat,
            q.Total,
            q.CreatedAt,
            q.CreatedByUserId,
            q.Notes!,
            q.Items.Select(i => new QuoteItemResponse(
                i.Id,
                i.ProductId,
                i.Product.Name,
                i.WidthMm,
                i.HeightMm,
                i.Material,
                i.Glazing,
                i.ColorTier,
                i.HardwareTier,
                i.InstallComplexity,
                i.Qty,
                i.UnitPrice,
                i.LineTotal
              ))
              .ToList()
          ));
          return Results.Ok(result);
        }
      )
      .Produces<IEnumerable<QuoteResponse>>(StatusCodes.Status200OK)
      .WithOpenApi(op =>
        op.Summarize("List quotes for a customer", "Returns quotes ordered by newest first.")
      );

    group
      .MapPost(
        "/quotes",
        async (
          ClaimsPrincipal user,
          [FromBody] CreateQuoteRequest req,
          IValidator<CreateQuoteRequest> v,
          AppDbContext db,
          IAiPricingClient ai
        ) =>
        {
          var val = await v.ValidateAsync(req);
          if (!val.IsValid)
            return Results.ValidationProblem(val.ToDictionary());

          var existsCustomer = await db.Customers.AnyAsync(c => c.Id == req.CustomerId);
          if (!existsCustomer)
            return Results.BadRequest(new { message = "Customer not found." });

          var sub =
            user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
          if (!int.TryParse(sub, out var creatorId))
          {
            return Results.Problem(
              title: "Missing user id",
              detail: "JWT is missing a valid 'sub' claim. Please sign in again.",
              statusCode: StatusCodes.Status401Unauthorized
            );
          }

          var quote = new Quote
          {
            CustomerId = req.CustomerId,
            CreatedByUserId = creatorId,
            Status = "Draft",
            Notes = req.Notes,
          };

          var productIds = req.Items.Select(i => i.ProductId).Distinct().ToList();
          var products = await db
            .Products.Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

          var missing = req
            .Items.Select(i => i.ProductId)
            .Where(id => !products.ContainsKey(id))
            .Distinct()
            .ToList();

          if (missing.Count > 0)
            return Results.BadRequest(new { message = $"Product {missing[0]} not found." });

          var needingAi = new List<(int idx, QuoteItemCreateRequest item)>();
          for (int i = 0; i < req.Items.Count; i++)
            if (!req.Items[i].UnitPrice.HasValue)
              needingAi.Add((i, req.Items[i]));

          if (needingAi.Count > 0)
          {
            var aiInputs = needingAi
              .Select(pair =>
              {
                var prod = products[pair.item.ProductId];
                return new AiQuoteItemIn(
                  prod.ProductType,
                  pair.item.WidthMm,
                  pair.item.HeightMm,
                  pair.item.Material,
                  pair.item.Glazing,
                  pair.item.ColorTier,
                  pair.item.HardwareTier,
                  pair.item.InstallComplexity,
                  pair.item.Qty
                );
              })
              .ToList();

            try
            {
              var aiPrices = await ai.PredictUnitPricesAsync(aiInputs);
              if (aiPrices.Count == needingAi.Count)
              {
                for (int j = 0; j < needingAi.Count; j++)
                {
                  var (idx, item) = needingAi[j];
                  req.Items[idx] = item with { UnitPrice = decimal.Round(aiPrices[j], 2) };
                }
              }
            }
            catch
            { /* fallback below */
            }
          }

          foreach (var it in req.Items)
          {
            if (!products.TryGetValue(it.ProductId, out var prod))
              return Results.BadRequest(new { message = $"Product {it.ProductId} not found." });

            var unit = it.UnitPrice ?? prod.BasePrice;
            var line = unit * it.Qty;
            var isAi = it.UnitPrice.HasValue;
            var conf = isAi ? 0.85 : (double?)null;

            quote.Items.Add(
              new QuoteItem
              {
                ProductId = it.ProductId,
                WidthMm = it.WidthMm,
                HeightMm = it.HeightMm,
                Material = it.Material,
                Glazing = it.Glazing,
                ColorTier = it.ColorTier,
                HardwareTier = it.HardwareTier,
                InstallComplexity = it.InstallComplexity,
                Qty = it.Qty,
                UnitPrice = unit,
                LineTotal = line,
                IsAiPriced = isAi,
                AiConfidence = conf,
              }
            );
          }

          QuoteCalculator.ComputeTotals(quote);
          db.Quotes.Add(quote);
          await db.SaveChangesAsync();

          await db.Entry(quote).Reference(q => q.Customer).LoadAsync();
          await db.Entry(quote).Collection(q => q.Items).LoadAsync();
          foreach (var i in quote.Items)
            await db.Entry(i).Reference(x => x.Product).LoadAsync();

          var resp = new QuoteResponse(
            quote.Id,
            quote.CustomerId,
            quote.Customer.Name,
            quote.Status,
            quote.Subtotal,
            quote.Vat,
            quote.Total,
            quote.CreatedAt,
            quote.CreatedByUserId,
            quote.Notes!,
            quote
              .Items.Select(i => new QuoteItemResponse(
                i.Id,
                i.ProductId,
                i.Product.Name,
                i.WidthMm,
                i.HeightMm,
                i.Material,
                i.Glazing,
                i.ColorTier,
                i.HardwareTier,
                i.InstallComplexity,
                i.Qty,
                i.UnitPrice,
                i.LineTotal
              ))
              .ToList()
          );

          return Results.Created($"/api/quotes/{quote.Id}", resp);
        }
      )
      .Produces<QuoteResponse>(StatusCodes.Status201Created)
      .ProducesValidationProblem()
      .WithOpenApi(op =>
        op.Summarize("Create quote", "Creates a quote; uses AI to backfill unit prices if missing.")
          .RequestExample(
            """
            {
              "customerId": 1,
              "notes": "Front windows and a door",
              "items": [
                {"productId":1,"widthMm":1200,"heightMm":900,"material":"uPVC","glazing":"double","colorTier":"Standard","hardwareTier":"Standard","installComplexity":"Standard","qty":2},
                {"productId":3,"widthMm":900,"heightMm":2100,"material":"Composite","glazing":"double","colorTier":"Premium","hardwareTier":"Premium","installComplexity":"Complex","qty":1}
              ]
            }
            """
          )
      );

    group
      .MapPost(
        "/quotes/summary",
        async (
          [FromBody] CreateQuoteRequest req,
          AppDbContext db,
          IHttpClientFactory httpFactory
        ) =>
        {
          var customer = await db.Customers.FindAsync(req.CustomerId);
          if (customer is null)
            return Results.BadRequest(new { message = "Customer not found." });

          var productIds = req.Items.Select(i => i.ProductId).Distinct().ToList();
          var products = await db
            .Products.Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

          var http = httpFactory.CreateClient("ai-raw");
          var aiReq = new
          {
            customer_name = customer.Name,
            items = req.Items.Select(i =>
            {
              var prod =
                products.GetValueOrDefault(i.ProductId)
                ?? throw new InvalidOperationException($"Product {i.ProductId} not found.");
              return new
              {
                product_type = prod.ProductType,
                width_mm = i.WidthMm,
                height_mm = i.HeightMm,
                material = i.Material,
                glazing = i.Glazing,
                color_tier = i.ColorTier,
                hardware_tier = i.HardwareTier,
                install_complexity = i.InstallComplexity,
                qty = i.Qty,
              };
            }),
            vat_rate = 0.20,
          };

          try
          {
            var resp = await http.PostAsJsonAsync("/summarize-quote", aiReq);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var text = body is not null && body.TryGetValue("text", out var t) ? t : string.Empty;
            return Results.Ok(new { text });
          }
          catch (Exception ex)
          {
            return Results.Problem(title: "AI summary failed", detail: ex.Message, statusCode: 502);
          }
        }
      )
      .Produces(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status502BadGateway)
      .WithOpenApi(op =>
        op.Summarize("Summarize quote", "Returns a readable summary for a draft quote.")
          .RequestExample(
            """
            {
              "customerId": 1,
              "items": [
                {"productId":1,"widthMm":1200,"heightMm":900,"material":"uPVC","glazing":"double","colorTier":"Standard","hardwareTier":"Standard","installComplexity":"Standard","qty":2}
              ]
            }
            """
          )
      );

    return app;
  }
}
