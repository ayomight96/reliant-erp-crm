using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Reliant.Application.DTOs;

namespace Reliant.Api.Tests;

public class SummaryEndpointTests : IClassFixture<ApiFactory>
{
  private readonly ApiFactory _factory;

  public SummaryEndpointTests(ApiFactory f) => _factory = f;

  private static async Task<HttpClient> CreateAuthedClientAsync(
    WebApplicationFactory<Program> factory
  )
  {
    var c = factory.CreateClient();

    var tokenResp = await c.PostAsJsonAsync(
      "/auth/login",
      new { Email = "sales@demo.local", Password = "Passw0rd!" }
    );

    tokenResp.EnsureSuccessStatusCode();

    var token = (await tokenResp.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    return c;
  }

  [Fact]
  public async Task Summary_Handles_Ai_Failure_With_502()
  {
    var failingFactory = _factory.WithWebHostBuilder(b =>
      b.ConfigureAppConfiguration(
        (_, cfg) =>
        {
          cfg.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
              ["AI:BaseUrl"] = "http://127.0.0.1:1", // unroutable → fast failure
            }
          );
        }
      )
    );

    var client = await CreateAuthedClientAsync(failingFactory);

    var payload = new
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
    };

    var resp = await client.PostAsJsonAsync("/api/quotes/summary", payload);

    resp.StatusCode.Should().Be(HttpStatusCode.BadGateway);
  }
}
