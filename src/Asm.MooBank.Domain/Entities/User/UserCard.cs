namespace Asm.MooBank.Domain.Entities.User;
public class UserCard
{
    public required Guid UserId { get; set; }

    public string? Name { get; set; }

    public required short Last4Digits { get; set; }

    public User User { get; set; } = null!;
}
