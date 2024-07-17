namespace Asm.MooBank.Tools.TransactionGenerator;

public class Transaction : ICloneable
{
    public DateTime Date { get; set; }

    public string Description { get; set; }

    public decimal? Credit { get; set; }

    public decimal? Debit { get; set; }

    public decimal Amount => Debit ?? Credit ?? throw new InvalidOperationException();

    public decimal Balance { get; set; }

    public int Frequency { get; set; }

    public int Day { get; set; }

    public int Month { get; set; }

    public bool FixedFrequency { get; set; }

    public bool FixedAmount { get; set; }

    public DateTime? LastDate { get; set; }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
