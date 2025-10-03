using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Reliant.Application.DTOs;

namespace Reliant.Api.Tests;

public class QuoteEndpointsTests : IClassFixture<ApiFactory>
{
  private readonly ApiFactory _factory;

  public QuoteEndpointsTests(ApiFactory f) => _factory = f;

  private async Task<HttpClient> AuthedAsync(string email = "sales@demo.local")
  {
    var c = _factory.CreateClient();
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      await TestAuth.GetTokenAsync(c, email)
    );
    return c;
  }

  [Fact]
  public async Task CreateQuote_Uses_BasePrice_When_UnitPrice_Provided()
  {
    var c = await AuthedAsync();
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
          qty = 1,
          unitPrice = 999.99m,
        },
      },
    };
    var resp = await c.PostAsJsonAsync("/api/quotes", req);
    resp.StatusCode.Should().Be(HttpStatusCode.Created);

    var quote = await resp.Content.ReadFromJsonAsync<QuoteResponse>();
    quote!.Items[0].UnitPrice.Should().Be(999.99m);
    quote.Items[0].LineTotal.Should().Be(999.99m);
  }

  [Fact]
  public async Task CreateQuote_ProductNotFound_400()
  {
    var c = await AuthedAsync();
    var req = new
    {
      customerId = 1,
      items = new[]
      {
        new
        {
          productId = 999999,
          widthMm = 1000,
          heightMm = 1000,
          material = "uPVC",
          glazing = "double",
          qty = 1,
        },
      },
    };
    var resp = await c.PostAsJsonAsync("/api/quotes", req);
    resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task ListQuotes_ByCustomer_Works()
  {
    var c = await AuthedAsync();
    // Ensure at least one quote exists
    await c.PostAsJsonAsync(
      "/api/quotes",
      new
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
            qty = 1,
          },
        },
      }
    );

    var resp = await c.GetAsync("/api/customers/1/quotes");
    resp.StatusCode.Should().Be(HttpStatusCode.OK);
    var list = await resp.Content.ReadFromJsonAsync<List<QuoteResponse>>();
    list!.Should().NotBeEmpty();
    list.All(q => q.CustomerId == 1).Should().BeTrue();
  }
}
