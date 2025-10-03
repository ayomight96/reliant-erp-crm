namespace Reliant.Domain.Entities;

public class QuoteItem
{
  public int Id { get; set; }
  public int QuoteId { get; set; }
  public Quote Quote { get; set; } = default!;
  public int ProductId { get; set; }
  public Product Product { get; set; } = default!;

  public int WidthMm { get; set; }
  public int HeightMm { get; set; }
  public string Material { get; set; } = default!; // uPVC/Aluminium
  public string Glazing { get; set; } = default!; // single/double/triple
  public string? ColorTier { get; set; } // Standard/Premium
  public string? HardwareTier { get; set; } // Standard/Premium
  public string? InstallComplexity { get; set; } // Easy/Standard/Complex

  public int Qty { get; set; }
  public decimal UnitPrice { get; set; } // from AI suggestion or manual
  public decimal LineTotal { get; set; }
  public bool IsAiPriced { get; set; } // default false
  public double? AiConfidence { get; set; } // 0..1
}
