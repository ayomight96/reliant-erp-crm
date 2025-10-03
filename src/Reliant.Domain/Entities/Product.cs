namespace Reliant.Domain.Entities;

public class Product
{
  public int Id { get; set; }
  public string Name { get; set; } = default!;
  public string ProductType { get; set; } = default!; // window/door/conservatory
  public decimal BasePrice { get; set; } // 12,2
}
