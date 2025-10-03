using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Reliant.Application.DTOs;
using Reliant.Domain.Entities;
using Reliant.Infrastructure;

namespace Reliant.Api.Endpoints;

public static class AuthEndpoints
{
  public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/auth").WithTags("Auth");

    group
      .MapPost(
        "/login",
        async ([FromBody] LoginRequest req, AppDbContext db, IConfiguration config) =>
        {
          var user = await db
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Email == req.Email && u.IsActive);

          if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Results.Unauthorized();

          var roles = user.UserRoles.Select(r => r.Role.Name).ToArray();

          var claims = new List<Claim>
          {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
          };
          claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
          claims.AddRange(roles.Select(r => new Claim("role", r)));

          var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "test-key-0123456789-0123456789-0123")
          );
          var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
          var expires = DateTime.UtcNow.AddHours(4);

          var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "reliant",
            audience: config["Jwt:Audience"] ?? "reliant.clients",
            claims: claims,
            expires: expires,
            signingCredentials: creds
          );

          var jwt = new JwtSecurityTokenHandler().WriteToken(token);
          return Results.Ok(new LoginResponse(jwt, expires, roles));
        }
      )
      .Produces<LoginResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .WithOpenApi(op =>
        op.Summarize("Login and get JWT", "Authenticates a user and returns a bearer token.")
          .RequestExample("""{"email":"sales@demo.local","password":"Passw0rd!"}""")
      );

    return app;
  }
}
