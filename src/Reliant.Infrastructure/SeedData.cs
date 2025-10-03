using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Reliant.Domain.Entities;

namespace Reliant.Infrastructure;

public static class SeedData
{
  public static async Task EnsureSchemaAsync(AppDbContext db, bool useMigrations)
  {
    if (useMigrations)
      await db.Database.MigrateAsync();
    else
      await db.Database.EnsureCreatedAsync();
  }

  public static async Task SeedAsync(AppDbContext db)
  {
    // Roles
    if (!await db.Roles.AnyAsync())
    {
      db.Roles.AddRange(new Role { Name = Roles.Manager }, new Role { Name = Roles.Sales });
      await db.SaveChangesAsync();
    }

    // Users
    if (!await db.Users.AnyAsync())
    {
      var users = new List<User>
      {
        new User
        {
          Email = "manager@demo.local",
          FullName = "Sarah Johnson",
          PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!"),
        },
        new User
        {
          Email = "sales@demo.local",
          FullName = "Mike Chen",
          PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!"),
        },
        new User
        {
          Email = "alex.williams@demo.local",
          FullName = "Alex Williams",
          PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!"),
        },
        new User
        {
          Email = "jessica.martinez@demo.local",
          FullName = "Jessica Martinez",
          PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!"),
        },
        new User
        {
          Email = "david.smith@demo.local",
          FullName = "David Smith",
          PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!"),
        },
        new User
        {
          Email = "emily.brown@demo.local",
          FullName = "Emily Brown",
          PasswordHash = BCrypt.Net.BCrypt.HashPassword("Passw0rd!"),
        },
      };

      db.Users.AddRange(users);
      await db.SaveChangesAsync();

      // Get roles
      var managerRole = await db.Roles.FirstAsync(r => r.Name == Roles.Manager);
      var salesRole = await db.Roles.FirstAsync(r => r.Name == Roles.Sales);

      var userRoles = new List<UserRole>
      {
        new UserRole { UserId = users[0].Id, RoleId = managerRole.Id }, // Sarah - Manager
        new UserRole { UserId = users[1].Id, RoleId = salesRole.Id }, // Mike - Sales
        new UserRole { UserId = users[2].Id, RoleId = salesRole.Id }, // Alex - Sales
        new UserRole { UserId = users[3].Id, RoleId = salesRole.Id }, // Jessica - Sales
        new UserRole { UserId = users[4].Id, RoleId = managerRole.Id }, // David - Manager
        new UserRole { UserId = users[5].Id, RoleId = managerRole.Id }, // Emily - Manager
      };

      db.UserRoles.AddRange(userRoles);
      await db.SaveChangesAsync();
    }

    // Customers
    if (!await db.Customers.AnyAsync())
    {
      var customers = GenerateCustomers(30);
      db.Customers.AddRange(customers);
      await db.SaveChangesAsync();
    }

    // Products
    if (!await db.Products.AnyAsync())
    {
      db.Products.AddRange(
        new Product
        {
          Name = "uPVC Casement Window",
          ProductType = "window",
          BasePrice = 220m,
        },
        new Product
        {
          Name = "uPVC Tilt-Turn Window",
          ProductType = "window",
          BasePrice = 260m,
        },
        new Product
        {
          Name = "Composite Door",
          ProductType = "door",
          BasePrice = 750m,
        },
        new Product
        {
          Name = "Aluminium Bi-fold Door (panel)",
          ProductType = "door",
          BasePrice = 600m,
        },
        new Product
        {
          Name = "Lean-to Conservatory Kit (m²)",
          ProductType = "conservatory",
          BasePrice = 350m,
        },
        new Product
        {
          Name = "Edwardian Conservatory Kit (m²)",
          ProductType = "conservatory",
          BasePrice = 420m,
        }
      );
      await db.SaveChangesAsync();
    }

    // Quotes (3 samples)
    if (!await db.Quotes.AnyAsync())
    {
      var salesId = await db
        .Users.Where(u => u.Email == "sales@demo.local")
        .Select(u => u.Id)
        .FirstAsync();

      // Get customers that exist from our generated list
      var customers = await db.Customers.Take(2).ToListAsync();
      var cust1 = customers[0];
      var cust2 = customers[1];

      var pWin = await db
        .Products.Where(p => p.ProductType == "window" && p.Name.Contains("Casement"))
        .FirstAsync();
      var pDoor = await db
        .Products.Where(p => p.ProductType == "door" && p.Name.Contains("Composite"))
        .FirstAsync();

      // Quote 1 (2 windows)
      var q1 = new Quote
      {
        CustomerId = cust1.Id,
        CreatedByUserId = salesId,
        Status = "Draft",
      };
      q1.Items.Add(
        new QuoteItem
        {
          ProductId = pWin.Id,
          WidthMm = 1200,
          HeightMm = 900,
          Material = "uPVC",
          Glazing = "double",
          ColorTier = "Standard",
          HardwareTier = "Standard",
          InstallComplexity = "Standard",
          Qty = 2,
          UnitPrice = 280m,
          LineTotal = 560m,
        }
      );
      CalcTotals(q1);

      // Quote 2 (door + window)
      var q2 = new Quote
      {
        CustomerId = cust2.Id,
        CreatedByUserId = salesId,
        Status = "Sent",
      };
      q2.Items.Add(
        new QuoteItem
        {
          ProductId = pDoor.Id,
          WidthMm = 900,
          HeightMm = 2100,
          Material = "Composite",
          Glazing = "double",
          ColorTier = "Premium",
          HardwareTier = "Premium",
          InstallComplexity = "Complex",
          Qty = 1,
          UnitPrice = 950m,
          LineTotal = 950m,
        }
      );
      q2.Items.Add(
        new QuoteItem
        {
          ProductId = pWin.Id,
          WidthMm = 1000,
          HeightMm = 1000,
          Material = "uPVC",
          Glazing = "triple",
          ColorTier = "Premium",
          HardwareTier = "Standard",
          InstallComplexity = "Standard",
          Qty = 1,
          UnitPrice = 320m,
          LineTotal = 320m,
        }
      );
      CalcTotals(q2);

      db.Quotes.AddRange(q1, q2);
      await db.SaveChangesAsync();
    }
  }

  private static List<Customer> GenerateCustomers(int count)
  {
    var cities = new[]
    {
      "Birmingham",
      "Wolverhampton",
      "Leicester",
      "Coventry",
      "Walsall",
      "Solihull",
      "Redditch",
      "Dudley",
    };
    var businessTypes = new[]
    {
      "Construction",
      "Legal",
      "Medical",
      "Automotive",
      "Restaurant",
      "Tech",
      "Manufacturing",
      "Logistics",
      "Dental",
      "Accounting",
    };
    var lastNames = new[]
    {
      "Smith",
      "Jones",
      "Patel",
      "Evans",
      "Khan",
      "Green",
      "Taylor",
      "Walker",
      "Brown",
      "Wilson",
      "Davis",
      "Miller",
      "Anderson",
      "Clark",
      "Rodriguez",
    };

    var random = new Random();
    var customers = new List<Customer>();

    for (int i = 0; i < count; i++)
    {
      var city = cities[random.Next(cities.Length)];
      var isBusiness = random.Next(2) == 0;

      if (isBusiness)
      {
        var businessType = businessTypes[random.Next(businessTypes.Length)];
        var lastName = lastNames[random.Next(lastNames.Length)];
        var businessName = $"{lastName} {businessType}";

        customers.Add(
          new Customer
          {
            Name = businessName,
            Email = $"info@{lastName.ToLower()}{businessType.ToLower()}.co.uk",
            City = city,
            Postcode = GeneratePostcode(city, random),
            Phone = GeneratePhoneNumber(city, random),
          }
        );
      }
      else
      {
        var title = random.Next(2) == 0 ? "Mr" : "Ms";
        var lastName = lastNames[random.Next(lastNames.Length)];
        var firstName =
          title == "Mr"
            ? new[] { "David", "Robert", "James", "Peter", "Martin", "Thomas" }[random.Next(6)]
            : new[] { "Sarah", "Jennifer", "Lisa", "Angela", "Chloe", "Rebecca" }[random.Next(6)];

        customers.Add(
          new Customer
          {
            Name = $"{title} {firstName} {lastName}",
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
            City = city,
            Postcode = GeneratePostcode(city, random),
            Phone = GeneratePhoneNumber(city, random),
          }
        );
      }
    }

    return customers;
  }

  private static string GeneratePostcode(string city, Random random)
  {
    var areaCodes = new Dictionary<string, string>
    {
      { "Birmingham", "B" },
      { "Wolverhampton", "WV" },
      { "Leicester", "LE" },
      { "Coventry", "CV" },
      { "Walsall", "WS" },
      { "Solihull", "B" },
      { "Redditch", "B" },
      { "Dudley", "DY" },
    };

    var area = areaCodes[city];
    var number = random.Next(1, 99);
    var letter = (char)('A' + random.Next(0, 26));
    var final = random.Next(1, 9);

    return $"{area}{number} {final}{letter}{letter}";
  }

  private static string GeneratePhoneNumber(string city, Random random)
  {
    var areaCodes = new Dictionary<string, string>
    {
      { "Birmingham", "0121" },
      { "Wolverhampton", "01902" },
      { "Leicester", "0116" },
      { "Coventry", "024" },
      { "Walsall", "01922" },
      { "Solihull", "0121" },
      { "Redditch", "01527" },
      { "Dudley", "01384" },
    };

    var areaCode = areaCodes[city];
    var number = $"{random.Next(100, 1000)} {random.Next(1000, 10000)}";
    return $"{areaCode} {number}";
  }

  private static void CalcTotals(Quote q)
  {
    q.Subtotal = q.Items.Sum(i => i.LineTotal);
    q.Vat = Math.Round(q.Subtotal * 0.20m, 2);
    q.Total = q.Subtotal + q.Vat;
    q.Notes = $"Auto-seeded quote for {q.Items.Count} item(s). Total £{q.Total:F2} inc VAT.";
  }
}
