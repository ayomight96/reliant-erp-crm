namespace Reliant.Application.DTOs;

public record QuoteItemCreateRequest(
  int ProductId,
  int WidthMm,
  int HeightMm,
  string Material,
  string Glazing,
  string? ColorTier,
  string? HardwareTier,
  string? InstallComplexity,
  int Qty,
  decimal? UnitPrice
);

public record CreateQuoteRequest(int CustomerId, List<QuoteItemCreateRequest> Items, string? Notes);

public record QuoteItemResponse(
  int Id,
  int ProductId,
  string ProductName,
  int WidthMm,
  int HeightMm,
  string Material,
  string Glazing,
  string? ColorTier,
  string? HardwareTier,
  string? InstallComplexity,
  int Qty,
  decimal UnitPrice,
  decimal LineTotal
);

public record QuoteResponse(
  int Id,
  int CustomerId,
  string CustomerName,
  string Status,
  decimal Subtotal,
  decimal Vat,
  decimal Total,
  DateTime CreatedAt,
  int CreatedByUserId,
  string? Notes,
  List<QuoteItemResponse> Items
);
