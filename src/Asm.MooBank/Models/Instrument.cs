namespace Asm.MooBank.Models;

public abstract record Instrument
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset BalanceDate { get; set; } = DateTimeOffset.Now;

    public required decimal CurrentBalance { get; set; }

    public required decimal? CurrentBalanceLocalCurrency { get; set; }

    public required string Currency { get; set; }

    public required Controller Controller { get; init; }

    public DateOnly? ClosedDate { get; set; }

    public string? InstrumentType { get; set; }

    public Guid? GroupId { get; set; }

    public virtual ICollection<VirtualInstrument> VirtualInstruments { get; init; } = [];

    /// <summary>
    /// Gets the remaining balance once virtual instruments are accounted for.
    /// </summary>
    public decimal? RemainingBalance { get; init; }

    /// <summary>
    /// Gets the remaining balance once virtual instruments are accounted for.
    /// </summary>
    public decimal? RemainingBalanceLocalCurrency { get; init; }
}
