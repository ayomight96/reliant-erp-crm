using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Reliant.Application.DTOs;

namespace Reliant.Api.Tests;

public class ProductEndpointsTests : IClassFixture<ApiFactory>
{
  private readonly ApiFactory _factory;

  public ProductEndpointsTests(ApiFactory f) => _factory = f;

  [Fact]
  public async Task Products_Have_Expected_Fields()
  {
    var c = _factory.CreateClient();
    var token = (
      await (
        await c.PostAsJsonAsync(
          "/auth/login",
          new { Email = "sales@demo.local", Password = "Passw0rd!" }
        )
      ).Content.ReadFromJsonAsync<LoginResponse>()
    )!.AccessToken;
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var resp = await c.GetAsync("/api/products");
    resp.StatusCode.Should().Be(HttpStatusCode.OK);

    var list = await resp.Content.ReadFromJsonAsync<List<ProductResponse>>();
    list!.Should().NotBeEmpty();
    list[0].Name.Should().NotBeNull();
    list[0].ProductType.Should().NotBeNull();
    list[0].BasePrice.Should().BeGreaterThan(0);
  }
}
