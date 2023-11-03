namespace Asm.MooBank.Modules.Account.Models.Account;

public record AccountsList
{
    public required IEnumerable<AccountListGroup> AccountGroups { get; init; }

    public decimal Position { get; init; }

}

public record AccountListGroup
{
    public required string Name { get; init; }

    public required IEnumerable<InstitutionAccount> Accounts { get; init; }
    public decimal? Position { get; set; }
}