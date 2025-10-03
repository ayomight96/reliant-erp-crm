namespace Reliant.Domain.Entities;

public class Customer
{
  public int Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Email { get; set; }
  public string? Phone { get; set; }
  public string? AddressLine1 { get; set; }
  public string? City { get; set; }
  public string? Postcode { get; set; }
  public string? Segment { get; set; } // e.g., Loyal, High-Potential, Dormant

  public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}
