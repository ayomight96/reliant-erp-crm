using System.Net;
using FluentAssertions;

namespace Reliant.Api.Tests;

public class SystemAndSwaggerTests : IClassFixture<ApiFactory>
{
  private readonly ApiFactory _factory;

  public SystemAndSwaggerTests(ApiFactory f) => _factory = f;

  [Fact]
  public async Task Healthz_Ok()
  {
    var c = _factory.CreateClient();
    var r = await c.GetAsync("/healthz");
    r.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task Debug_Counts_Returns_200()
  {
    var c = _factory.CreateClient();

    var token = await TestAuth.GetTokenAsync(c);
    c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
      "Bearer",
      token
    );

    var r = await c.GetAsync("/debug/counts");
    r.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task SwaggerJson_Exposes_Bearer_Scheme()
  {
    var c = _factory.CreateClient();
    var r = await c.GetAsync("/swagger/v1/swagger.json");
    r.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await r.Content.ReadAsStringAsync();
    body.Should().Contain("\"securitySchemes\"");
    body.Should().Contain("\"Bearer\"");
  }
}
