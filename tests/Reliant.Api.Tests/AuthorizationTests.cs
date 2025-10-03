using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;

namespace Reliant.Api.Tests;

public class AuthorizationTests : IClassFixture<ApiFactory>
{
  private readonly ApiFactory _factory;

  public AuthorizationTests(ApiFactory f) => _factory = f;

  [Fact]
  public async Task Products_Unauthenticated_401()
  {
    var c = _factory.CreateClient();
    var r = await c.GetAsync("/api/products");
    r.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Products_As_Sales_200()
  {
    var c = _factory.CreateClient();
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      await TestAuth.GetTokenAsync(c, "sales@demo.local")
    );
    var r = await c.GetAsync("/api/products");
    r.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task Admin_AssignRole_Requires_Manager()
  {
    var c = _factory.CreateClient();
    // Sales tries: should be 403
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      await TestAuth.GetTokenAsync(c, "sales@demo.local")
    );
    var r1 = await c.PostAsync("/admin/users/1/roles/Sales", null);
    r1.StatusCode.Should().Be(HttpStatusCode.Forbidden);

    // Manager can do it
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      await TestAuth.GetTokenAsync(c, "manager@demo.local")
    );
    var r2 = await c.PostAsync("/admin/users/1/roles/Sales", null);
    r2.StatusCode.Should().Be(HttpStatusCode.NoContent);
  }
}
