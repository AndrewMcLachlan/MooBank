namespace Asm.MooBank.Modules.Bills.Models;

public record ImportBill : BillBase
{
    /// <summary>
    /// The name of the account to import the bill to.
    /// </summary>
    public required string AccountName { get; set; }
}

public record ImportResult
{
    public int Imported { get; init; }

    public int Failed { get; init; }

    public IEnumerable<string> Errors { get; init; } = [];
}
