namespace Asm.MooBank.Modules.AccountHolder.Models;

public record AccountHolderCard
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public required short Last4Digits { get; set; }
}
