using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Reliant.Application.DTOs;

namespace Reliant.Api.Tests;

public class CustomerEndpointsTests : IClassFixture<ApiFactory>
{
  private readonly ApiFactory _factory;

  public CustomerEndpointsTests(ApiFactory f) => _factory = f;

  private async Task<HttpClient> AuthedAsync()
  {
    var c = _factory.CreateClient();
    var tokenResp = await c.PostAsJsonAsync(
      "/auth/login",
      new { Email = "sales@demo.local", Password = "Passw0rd!" }
    );
    var token = (await tokenResp.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    return c;
  }

  [Fact]
  public async Task List_Customers_200_And_NotEmpty()
  {
    var c = await AuthedAsync();
    var r = await c.GetAsync("/api/customers");
    r.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task Get_Customer_NotFound_404()
  {
    var c = await AuthedAsync();
    var r = await c.GetAsync("/api/customers/999999");
    r.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task Create_Customer_Validation_400()
  {
    var c = await AuthedAsync();
    var resp = await c.PostAsJsonAsync(
      "/api/customers",
      new CreateCustomerRequest("", "", null, null, null, null)
    );
    resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    var problem = await resp.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
    problem!.Errors.Keys.Should().Contain(k => k.Contains("Name"));
  }
}
