using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reliant.Application.DTOs;
using Reliant.Application.Validation;
using Reliant.Domain.Entities;
using Reliant.Infrastructure;

namespace Reliant.Api.Endpoints;

public static class CustomerEndpoints
{
  public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/customers")
      .WithTags("Customers")
      .RequireAuthorization("SalesOrManager");

    group
      .MapGet(
        "/",
        async ([FromQuery] string? q, AppDbContext db) =>
        {
          var query = db.Customers.AsQueryable();
          if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c =>
              c.Name.Contains(q) || (c.Email != null && c.Email.Contains(q))
            );

          var list = await query
            .OrderBy(c => c.Name)
            .Select(c => new CustomerResponse(
              c.Id,
              c.Name,
              c.Email,
              c.Phone,
              c.AddressLine1,
              c.City,
              c.Postcode,
              c.Segment
            ))
            .ToListAsync();

          return Results.Ok(list);
        }
      )
      .Produces<IEnumerable<CustomerResponse>>(StatusCodes.Status200OK)
      .WithOpenApi(op =>
        op.Summarize("List customers", "Returns customers; optional search on name/email.")
          .AddQueryParam("q", "Search term (name or email)", "smith")
      );

    group
      .MapGet(
        "/{id:int}",
        async (int id, AppDbContext db) =>
        {
          var c = await db.Customers.FindAsync(id);
          return c is null
            ? Results.NotFound()
            : Results.Ok(
              new CustomerResponse(
                c.Id,
                c.Name,
                c.Email,
                c.Phone,
                c.AddressLine1,
                c.City,
                c.Postcode,
                c.Segment
              )
            );
        }
      )
      .Produces<CustomerResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound)
      .WithOpenApi(op => op.Summarize("Get customer", "Returns a single customer by id."));

    group
      .MapPost(
        "/",
        async (
          [FromBody] CreateCustomerRequest req,
          IValidator<CreateCustomerRequest> v,
          AppDbContext db
        ) =>
        {
          var val = await v.ValidateAsync(req);
          if (!val.IsValid)
            return Results.ValidationProblem(val.ToDictionary());

          var entity = new Customer
          {
            Name = req.Name,
            Email = req.Email,
            Phone = req.Phone,
            AddressLine1 = req.AddressLine1,
            City = req.City,
            Postcode = req.Postcode,
          };
          db.Customers.Add(entity);
          await db.SaveChangesAsync();

          return Results.Created(
            $"/api/customers/{entity.Id}",
            new CustomerResponse(
              entity.Id,
              entity.Name,
              entity.Email,
              entity.Phone,
              entity.AddressLine1,
              entity.City,
              entity.Postcode,
              entity.Segment
            )
          );
        }
      )
      .Produces<CustomerResponse>(StatusCodes.Status201Created)
      .ProducesValidationProblem()
      .WithOpenApi(op =>
        op.Summarize("Create customer", "Creates a new customer.")
          .RequestExample("""{"name":"Jones Ltd","email":"info@jonesltd.co.uk"}""")
      );

    group
      .MapPut(
        "/{id:int}",
        async (
          int id,
          [FromBody] UpdateCustomerRequest req,
          IValidator<UpdateCustomerRequest> v,
          AppDbContext db
        ) =>
        {
          var val = await v.ValidateAsync(req);
          if (!val.IsValid)
            return Results.ValidationProblem(val.ToDictionary());

          var c = await db.Customers.FindAsync(id);
          if (c is null)
            return Results.NotFound();

          c.Name = req.Name;
          c.Email = req.Email;
          c.Phone = req.Phone;
          c.AddressLine1 = req.AddressLine1;
          c.City = req.City;
          c.Postcode = req.Postcode;
          await db.SaveChangesAsync();
          return Results.NoContent();
        }
      )
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status404NotFound)
      .ProducesValidationProblem()
      .WithOpenApi(op => op.Summarize("Update customer", "Updates an existing customer."));

    return app;
  }
}
