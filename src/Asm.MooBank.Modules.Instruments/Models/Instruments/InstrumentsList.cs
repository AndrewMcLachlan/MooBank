using System.ComponentModel;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Instruments.Models.Instruments;

public record InstrumentsList
{
    public required IEnumerable<Group> Groups { get; init; }

    public decimal Total { get; init; }

}

[DisplayName("InstrumentGroup")]
public record Group
{
    public required string Name { get; init; }

    public required IEnumerable<Instrument> Instruments { get; init; }
    public decimal? Total { get; set; }
}
