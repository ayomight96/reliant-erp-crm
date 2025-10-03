using Microsoft.EntityFrameworkCore;
using Reliant.Domain.Entities;

namespace Reliant.Infrastructure;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options) { }

  public DbSet<User> Users => Set<User>();
  public DbSet<Role> Roles => Set<Role>();
  public DbSet<UserRole> UserRoles => Set<UserRole>();
  public DbSet<Customer> Customers => Set<Customer>();
  public DbSet<Product> Products => Set<Product>();
  public DbSet<Quote> Quotes => Set<Quote>();
  public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();

  protected override void OnModelCreating(ModelBuilder b)
  {
    // Users
    b.Entity<User>(e =>
    {
      e.HasIndex(x => x.Email).IsUnique();
      e.Property(x => x.Email).HasMaxLength(256);
    });

    // Roles
    b.Entity<Role>(e =>
    {
      e.HasIndex(x => x.Name).IsUnique();
      e.Property(x => x.Name).HasMaxLength(64);
    });

    // UserRoles (many-to-many)
    b.Entity<UserRole>(e =>
    {
      e.HasKey(x => new { x.UserId, x.RoleId });
      e.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId);
      e.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId);
    });

    // Customers
    b.Entity<Customer>(e =>
    {
      e.Property(x => x.Name).HasMaxLength(200).IsRequired();
      e.Property(x => x.Email).HasMaxLength(256);
      e.HasIndex(x => x.Email);
    });

    // Products
    b.Entity<Product>(e =>
    {
      e.Property(x => x.Name).HasMaxLength(200).IsRequired();
      e.Property(x => x.ProductType).HasMaxLength(100).IsRequired();
      e.Property(x => x.BasePrice).HasPrecision(12, 2);
    });

    // Quotes
    b.Entity<Quote>(e =>
    {
      e.Property(x => x.Status).HasMaxLength(50);
      e.Property(x => x.Subtotal).HasPrecision(12, 2);
      e.Property(x => x.Vat).HasPrecision(12, 2);
      e.Property(x => x.Total).HasPrecision(12, 2);

      e.HasOne(x => x.Customer).WithMany(c => c.Quotes).HasForeignKey(x => x.CustomerId);
      e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId);
    });

    // QuoteItems
    b.Entity<QuoteItem>(e =>
    {
      e.Property(x => x.UnitPrice).HasPrecision(12, 2);
      e.Property(x => x.LineTotal).HasPrecision(12, 2);
      e.Property(x => x.Material).HasMaxLength(50);
      e.Property(x => x.Glazing).HasMaxLength(50);
      e.Property(x => x.ColorTier).HasMaxLength(50);
      e.Property(x => x.HardwareTier).HasMaxLength(50);
      e.Property(x => x.InstallComplexity).HasMaxLength(50);

      e.HasOne(x => x.Quote).WithMany(q => q.Items).HasForeignKey(x => x.QuoteId);
      e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
    });
  }
}
