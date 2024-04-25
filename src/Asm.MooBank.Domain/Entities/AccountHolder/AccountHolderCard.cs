namespace Asm.MooBank.Domain.Entities.AccountHolder;
public class AccountHolderCard
{
    public required Guid AccountHolderId { get; set; }

    public string? Name { get; set; }

    public required short Last4Digits { get; set; }

    public User AccountHolder { get; set; } = null!;
}
