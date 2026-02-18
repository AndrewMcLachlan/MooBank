using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Instruments.Models.Instruments;

internal static class InstrumentExtensions
{
}

public record InstrumentSummary : Instrument
{
    public decimal VirtualAccountRemainingBalance
    {
        get => CurrentBalance - (VirtualInstruments?.Sum(v => v.CurrentBalance) ?? 0);
    }
}
