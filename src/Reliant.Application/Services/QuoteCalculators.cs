using Reliant.Domain.Entities;

namespace Reliant.Application.Services;

public static class QuoteCalculator
{
  public static void ComputeTotals(Quote q)
  {
    q.Subtotal = q.Items.Sum(i => i.LineTotal);
    q.Vat = Math.Round(q.Subtotal * 0.20m, 2);
    q.Total = q.Subtotal + q.Vat;
  }
}
