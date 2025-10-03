namespace Reliant.Application.DTOs;

public record UserListItemDto(int Id, string FullName, string Email, List<string> Roles);
