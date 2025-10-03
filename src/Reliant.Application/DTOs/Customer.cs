namespace Reliant.Application.DTOs;

public record CreateCustomerRequest(string Name, string? Email, string? Phone,
    string? AddressLine1, string? City, string? Postcode);

public record UpdateCustomerRequest(string Name, string? Email, string? Phone,
    string? AddressLine1, string? City, string? Postcode);

public record CustomerResponse(int Id, string Name, string? Email, string? Phone,
    string? AddressLine1, string? City, string? Postcode, string? Segment);
