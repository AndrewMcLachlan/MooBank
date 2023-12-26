namespace Asm.MooBank.Modules.Stock.Models;

/// <summary>
/// Duplicate, to be fixed
/// </summary>
public abstract record Account
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset BalanceDate { get; set; } = DateTimeOffset.Now;

    public required decimal CurrentBalance { get; set; }

    public string? AccountType { get; set; }

    public Guid? AccountGroupId { get; set; }
}