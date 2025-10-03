namespace Reliant.Domain.Entities;

public class Quote
{
  public int Id { get; set; }
  public int CustomerId { get; set; }
  public Customer Customer { get; set; } = default!;
  public string Status { get; set; } = "Draft"; // Draft/Sent/Accepted/Rejected
  public decimal Subtotal { get; set; }
  public decimal Vat { get; set; }
  public decimal Total { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public int CreatedByUserId { get; set; }
  public User CreatedByUser { get; set; } = default!;
  public string? Notes { get; set; } // summary text

  public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
}
