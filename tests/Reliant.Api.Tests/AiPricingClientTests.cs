using System.Net;
using System.Text.Json;
using FluentAssertions;
using Reliant.Api.Integration.AiPricing;

namespace Reliant.Api.Tests;

public class AiPricingClientTests
{
  [Fact]
  public async Task PredictUnitPrices_Maps_Response_Correctly()
  {
    var json = JsonSerializer.Serialize(
      new { items = new[] { new { unit_price = 123.45m, confidence = 0.9 } } }
    );
    var handler = new StubHandler(
      new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) }
    );
    var http = new HttpClient(handler) { BaseAddress = new Uri("http://example") };

    var client = new AiPricingClient(http);
    var prices = await client.PredictUnitPricesAsync(
      new[] { new AiQuoteItemIn("window", 1200, 900, "uPVC", "double", null, null, null, 1) }
    );

    prices.Should().ContainSingle().Which.Should().Be(123.45m);
  }

  private sealed class StubHandler : HttpMessageHandler
  {
    private readonly HttpResponseMessage _resp;

    public StubHandler(HttpResponseMessage resp) => _resp = resp;

    protected override Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request,
      CancellationToken cancellationToken
    ) => Task.FromResult(_resp);
  }
}
