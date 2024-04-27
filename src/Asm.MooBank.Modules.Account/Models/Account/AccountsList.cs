namespace Asm.MooBank.Modules.Accounts.Models.Account;

public record AccountsList
{
    public required IEnumerable<AccountListGroup> Groups { get; init; }

    public decimal Position { get; init; }

}

public record AccountListGroup
{
    public required string Name { get; init; }

    public required IEnumerable<Instrument> Accounts { get; init; }
    public decimal? Position { get; set; }
}
