using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Reliant.Application.DTOs;
using Reliant.Domain.Entities;
using Reliant.Infrastructure;

namespace Reliant.Api.Endpoints;

public static class AdminEndpoints
{
  public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/admin").WithTags("Admin").RequireAuthorization("ManagerOnly");

    group
      .MapPost(
        "/users/{userId:int}/roles/{roleName}",
        async (int userId, string roleName, AppDbContext db) =>
        {
          var user = await db
            .Users.Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId);
          if (user is null)
            return Results.NotFound(new { message = "User not found" });

          var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
          if (role is null)
            return Results.NotFound(new { message = "Role not found" });

          if (user.UserRoles.Any(ur => ur.RoleId == role.Id))
            return Results.NoContent();

          user.UserRoles.Clear();

          user.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id });
          await db.SaveChangesAsync();
          return Results.NoContent();
        }
      )
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status404NotFound)
      .WithOpenApi(op => op.Summarize("Assign role", "Assigns a role to a user."));

    group
      .MapGet(
        "/users",
        async (AppDbContext db) =>
        {
          var users = await db
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.Id)
            .Select(u => new UserListItemDto(
              u.Id,
              u.FullName ?? "",
              u.Email,
              u.UserRoles.Select(ur => ur.Role.Name).ToList()
            ))
            .ToListAsync();

          return Results.Ok(users);
        }
      )
      .Produces<IEnumerable<UserListItemDto>>(StatusCodes.Status200OK)
      .WithOpenApi(op => op.Summarize("List users", "Returns all users with their roles."));

    return app;
  }
}
