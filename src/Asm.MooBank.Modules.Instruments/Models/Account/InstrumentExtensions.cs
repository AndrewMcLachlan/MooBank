using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Instruments.Models.Account;
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
