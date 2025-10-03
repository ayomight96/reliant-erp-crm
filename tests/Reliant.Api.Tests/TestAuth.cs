using System.Net.Http.Json;
using Microsoft.IdentityModel.Tokens;
using Reliant.Application.DTOs;

public static class TestAuth
{
  public static async Task<string> GetTokenAsync(HttpClient c, string? email = null)
  {
    var resp = await c.PostAsJsonAsync(
      "/auth/login",
      new { Email = email.IsNullOrEmpty() ? "sales@demo.local" : email, Password = "Passw0rd!" }
    );
    resp.EnsureSuccessStatusCode();
    var body = await resp.Content.ReadFromJsonAsync<LoginResponse>();
    return body!.AccessToken;
  }
}
