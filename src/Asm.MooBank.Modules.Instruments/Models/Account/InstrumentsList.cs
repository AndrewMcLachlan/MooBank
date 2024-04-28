using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Instruments.Models.Account;

public record InstrumentsList
{
    public required IEnumerable<Group> Groups { get; init; }

    public decimal Total { get; init; }

}

public record Group
{
    public required string Name { get; init; }

    public required IEnumerable<Instrument> Instruments { get; init; }
    public decimal? Total { get; set; }
}
