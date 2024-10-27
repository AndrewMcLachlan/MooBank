using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Transactions.Models;

public partial record Transaction
{
    public Guid Id { get; set; } = new Guid();
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public string? Location { get; set; }

    public string? AccountHolderName { get; set; }

    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public bool ExcludeFromReporting { get; set; }
    public DateTime TransactionTime { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public TransactionType TransactionType { get; set; }

    public IEnumerable<Tag> Tags { get; set; } = Enumerable.Empty<Tag>();

    public IEnumerable<TransactionSplit> Splits { get; set; } = Enumerable.Empty<TransactionSplit>();

    public IEnumerable<TransactionOffsetFor> OffsetFor { get; set; } = Enumerable.Empty<TransactionOffsetFor>();

    public object? ExtraInfo { get; set; }
}
