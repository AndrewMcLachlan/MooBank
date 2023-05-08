namespace Asm.MooBank.Models;

public partial record Transaction
{
    public Guid Id { get; set; } = new Guid();
    public Guid? Reference { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal NetAmount { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime TransactionTime { get; set; }

    public TransactionType TransactionType { get; set; }

    public IEnumerable<TransactionTag> Tags { get; set; }

    public Transaction? Offsets { get; set; }

    public Transaction? OffsetBy { get; set; }

    public object? ExtraInfo { get; set; }
}
