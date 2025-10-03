using FluentAssertions;
using Reliant.Application.Services;
using Reliant.Domain.Entities;

public class QuoteCalculatorTests
{
  [Fact]
  public void ComputeTotals_Works()
  {
    var q = new Quote();
    q.Items.Add(new QuoteItem { LineTotal = 100m });
    q.Items.Add(new QuoteItem { LineTotal = 50m });

    QuoteCalculator.ComputeTotals(q);

    q.Subtotal.Should().Be(150m);
    q.Vat.Should().Be(30m);
    q.Total.Should().Be(180m);
  }
}
