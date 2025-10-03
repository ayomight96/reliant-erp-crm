namespace Reliant.Domain.Entities;

public class User
{
  public int Id { get; set; }
  public string Email { get; set; } = default!;
  public string PasswordHash { get; set; } = default!;
  public string? FullName { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public bool IsActive { get; set; } = true;

  public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
