using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Reliant.Application.DTOs;

namespace Reliant.Api.Tests;

public class AuthTests : IClassFixture<ApiFactory>
{
  private readonly ApiFactory _factory;

  public AuthTests(ApiFactory f) => _factory = f;

  [Fact]
  public async Task Login_Succeeds_And_Returns_Token_With_Roles()
  {
    var c = _factory.CreateClient();
    var resp = await c.PostAsJsonAsync(
      "/auth/login",
      new { Email = "sales@demo.local", Password = "Passw0rd!" }
    );
    resp.StatusCode.Should().Be(HttpStatusCode.OK);

    var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
    body!.AccessToken.Should().NotBeNullOrWhiteSpace();
    body.Roles.Should().Contain("Sales");
  }

  [Fact]
  public async Task Login_BadCreds_401()
  {
    var c = _factory.CreateClient();
    var resp = await c.PostAsJsonAsync(
      "/auth/login",
      new { Email = "sales@demo.local", Password = "nope" }
    );
    resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }
}
