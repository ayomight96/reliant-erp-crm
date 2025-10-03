using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reliant.Api.Integration.AiPricing;

public record AiQuoteItemIn(
  string product_type,
  int width_mm,
  int height_mm,
  string material,
  string glazing,
  string? color_tier,
  string? hardware_tier,
  string? install_complexity,
  int qty
);

public record AiPredictBatchRequest(List<AiQuoteItemIn> items);

public record AiPredictItemOut(
  [property: JsonPropertyName("unit_price")] decimal unit_price,
  [property: JsonPropertyName("confidence")] double confidence
// server may send extra fields (e.g., "features"); System.Text.Json ignores them by default
);

public record AiPredictBatchResponse(List<AiPredictItemOut> items);

public interface IAiPricingClient
{
  Task<IReadOnlyList<decimal>> PredictUnitPricesAsync(
    IEnumerable<AiQuoteItemIn> items,
    CancellationToken ct = default
  );
}

public sealed class AiPricingClient : IAiPricingClient
{
  private readonly HttpClient _http;
  private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

  public AiPricingClient(HttpClient http) => _http = http;

  public async Task<IReadOnlyList<decimal>> PredictUnitPricesAsync(
    IEnumerable<AiQuoteItemIn> items,
    CancellationToken ct = default
  )
  {
    var payload = new AiPredictBatchRequest(items.ToList());
    using var resp = await _http.PostAsJsonAsync("/predict-quote/batch", payload, JsonOpts, ct);
    resp.EnsureSuccessStatusCode();

    var body =
      await resp.Content.ReadFromJsonAsync<AiPredictBatchResponse>(JsonOpts, ct)
      ?? throw new InvalidOperationException("Empty AI response");

    return body.items.Select(i => i.unit_price).ToList();
  }
}
