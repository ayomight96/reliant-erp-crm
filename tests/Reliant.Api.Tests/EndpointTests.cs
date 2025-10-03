using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Reliant.Application.DTOs;

namespace Reliant.Api.Tests;

public class EndpointsTests : IClassFixture<ApiFactory>
{
  private readonly ApiFactory _factory;

  public EndpointsTests(ApiFactory factory) => _factory = factory;

  [Fact]
  public async Task CreateQuote_Uses_Ai_When_UnitPrice_Missing()
  {
    var c = _factory.CreateClient();
    var token = await TestAuth.GetTokenAsync(c);
    c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
      "Bearer",
      token
    );

    var req = new
    {
      customerId = 1,
      items = new[]
      {
        new
        {
          productId = 1,
          widthMm = 1200,
          heightMm = 900,
          material = "uPVC",
          glazing = "double",
          qty = 2,
        },
      },
    };

    var resp = await c.PostAsJsonAsync("/api/quotes", req);
    resp.StatusCode.Should().Be(HttpStatusCode.Created);

    var quote = await resp.Content.ReadFromJsonAsync<QuoteResponse>();
    quote!.Items[0].UnitPrice.Should().Be(321.23m); // from FakeAiPricingClient
    quote.Total.Should().BeGreaterThan(0);
  }
}
