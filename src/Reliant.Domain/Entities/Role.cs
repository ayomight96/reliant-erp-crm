namespace Reliant.Domain.Entities;

public class Role
{
  public int Id { get; set; }
  public string Name { get; set; } = default!; // "Manager", "Sales"
  public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public static class Roles
{
  public const string Manager = "Manager";
  public const string Sales = "Sales";
}
