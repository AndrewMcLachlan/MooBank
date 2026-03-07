namespace Asm.MooBank.Modules.Users.Models;

public record UserCard
{
    public string? Name { get; set; }

    public required short Last4Digits { get; set; }
}
