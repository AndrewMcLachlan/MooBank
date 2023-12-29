namespace Asm.MooBank.Modules.Account.Models.Account;

public abstract record Account
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset BalanceDate { get; set; } = DateTimeOffset.Now;

    public required decimal CurrentBalance { get; set; }

    public required decimal? CurrentBalanceLocalCurrency { get; set; }

    public required string Currency { get; set; }

    public string? AccountType { get; set; }

    public Guid? AccountGroupId { get; set; }

    public IEnumerable<VirtualAccount> VirtualAccounts { get; set; } = [];
}