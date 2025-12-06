using System.ComponentModel;
using Asm.MooBank.Modules.Instruments.Models.Instruments;
using Asm.MooBank.Modules.Instruments.Queries.Instruments;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Asm.MooBank.Modules.Transactions.McpTools;

[McpServerToolType]
public class InstrumentTools(IQueryDispatcher queryDispatcher)
{
    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-instruments", ReadOnly = true, Title = "Get Instruments")]
    [Description("Retrieves the list of financial instruments (bank accounts, loands, superannuation accounts, shares, assets etc.)")]
    public ValueTask<InstrumentsList> GetInstruments()
    {
        return queryDispatcher.Dispatch(new GetFormatted());
    }
}
