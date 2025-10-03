using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Reliant.Api.Tests;

public class SummaryEndpointSuccessTests : IClassFixture<ApiFactory>
{
  private readonly ApiFactory _factory;

  public SummaryEndpointSuccessTests(ApiFactory f) => _factory = f;

  [Fact]
  public async Task Summary_Happy_Path_200()
  {
    var factory = _factory.WithWebHostBuilder(b =>
    {
      b.ConfigureServices(services =>
      {
        // Swap named client "ai-raw" to use a stub handler
        services
          .AddHttpClient("ai-raw")
          .ConfigurePrimaryHttpMessageHandler(() =>
            new StubHandler(
              new HttpResponseMessage(HttpStatusCode.OK)
              {
                Content = new StringContent(
                  JsonSerializer.Serialize(new { text = "Hello summary" })
                ),
              }
            )
          );
      });
    });

    var c = factory.CreateClient();
    var resp = await c.PostAsJsonAsync(
      "/auth/login",
      new { Email = "sales@demo.local", Password = "Passw0rd!" }
    );
    var token = (
      await resp.Content.ReadFromJsonAsync<Reliant.Application.DTOs.LoginResponse>()
    )!.AccessToken;
    c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
      "Bearer",
      token
    );

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

    var r = await c.PostAsJsonAsync("/api/quotes/summary", payload);
    r.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await r.Content.ReadFromJsonAsync<Dictionary<string, string>>();
    body!["text"].Should().Contain("Hello summary");
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
