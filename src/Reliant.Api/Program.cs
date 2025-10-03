// Program.cs
using System.Reflection;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Reliant.Api.Endpoints;
using Reliant.Api.Integration.AiPricing;
using Reliant.Application.DTOs;
using Reliant.Application.Validation;
using Reliant.Domain.Entities;
using Reliant.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// MVC + validators
builder.Services.AddControllers();
builder.Services.AddScoped<IValidator<CreateCustomerRequest>, CreateCustomerRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateCustomerRequest>, UpdateCustomerRequestValidator>();
builder.Services.AddScoped<IValidator<QuoteItemCreateRequest>, QuoteItemCreateRequestValidator>();
builder.Services.AddScoped<IValidator<CreateQuoteRequest>, CreateQuoteRequestValidator>();

// AI client
builder.Services.AddHttpClient<IAiPricingClient, AiPricingClient>(c =>
{
  var baseUrl =
    builder.Configuration["AI:BaseUrl"] ?? builder.Configuration["AI__BaseUrl"] ?? "http://ai:8000";
  c.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient(
  "ai-raw",
  c =>
  {
    var baseUrl =
      builder.Configuration["AI:BaseUrl"]
      ?? builder.Configuration["AI__BaseUrl"]
      ?? "http://ai:8000";
    c.BaseAddress = new Uri(baseUrl);
    c.Timeout = TimeSpan.FromSeconds(3);
  }
);

// Db (Npgsql in Docker via ConnectionStrings__Default, else SQLite)
builder.Services.AddAppDb(builder.Configuration);

// AuthN/Z
builder
  .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    var key = builder.Configuration["Jwt:Key"] ?? "test-key-0123456789-0123456789-0123";
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "reliant",
      ValidateAudience = true,
      ValidAudience = builder.Configuration["Jwt:Audience"] ?? "reliant.clients",
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
      ValidateLifetime = true,
      ClockSkew = TimeSpan.FromMinutes(1),
    };
  });

builder.Services.AddAuthorization(options =>
{
  options.AddPolicy("ManagerOnly", p => p.RequireRole(Roles.Manager));
  options.AddPolicy("SalesOrManager", p => p.RequireRole(Roles.Sales, Roles.Manager));
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc(
    "v1",
    new OpenApiInfo
    {
      Title = "Reliant ERP/CRM API",
      Version = "v1",
      Description = "Quotations, customers, role-based access + AI pricing & summaries.",
      Contact = new OpenApiContact
      {
        Name = "Reliant Windows",
        Url = new Uri("https://example.com"),
      },
    }
  );

  // Bearer auth
  c.AddSecurityDefinition(
    "Bearer",
    new OpenApiSecurityScheme
    {
      Description =
        "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
      Name = "Authorization",
      In = ParameterLocation.Header,
      Type = SecuritySchemeType.Http,
      Scheme = "bearer",
      BearerFormat = "JWT",
    }
  );
  c.AddSecurityRequirement(
    new OpenApiSecurityRequirement
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
        },
        Array.Empty<string>()
      },
    }
  );

  // Include XML comments from *all* built assemblies in /bin
  foreach (var xml in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml"))
    c.IncludeXmlComments(xml, includeControllerXmlComments: true);
});

var app = builder.Build();

// Auto-migrate + seed
using (var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  var useMigrations = !app.Environment.IsEnvironment("Testing");
  await SeedData.EnsureSchemaAsync(db, useMigrations);
  await SeedData.SeedAsync(db);
}

// Pipeline
app.UseSwagger();
app.UseSwaggerUI(o =>
{
  o.SwaggerEndpoint("/swagger/v1/swagger.json", "Reliant ERP/CRM API v1");
  o.DocumentTitle = "Reliant ERP/CRM – API Docs";
  o.DisplayRequestDuration();
});

app.UseAuthentication();
app.UseAuthorization();

// Routes moved to modules
app.MapSystemEndpoints();
app.MapAuthEndpoints();
app.MapCustomerEndpoints();
app.MapProductEndpoints();
app.MapQuoteEndpoints();
app.MapAdminEndpoints();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

public partial class Program { }
