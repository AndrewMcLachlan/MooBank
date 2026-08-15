using System.ComponentModel;
using Asm.MooBank.Modules.Bills.Models;
using ModelContextProtocol.Server;

namespace Asm.MooBank.Modules.Bills.McpTools;

[McpServerToolType]
public class BillTools(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
{
    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-bill-accounts", ReadOnly = true, Title = "Get Bill Accounts")]
    [Description("Retrieves the utility accounts (electricity, gas, water etc.) that bills can be recorded against, along with the date range already covered by recorded bills. Call this first: import-bills identifies accounts by name and only the names returned here will match.")]
    public ValueTask<IEnumerable<Models.Account>> GetBillAccounts(CancellationToken cancellationToken = default) =>
        queryDispatcher.Dispatch(new Queries.Accounts.GetAll(), cancellationToken);

    [McpServerTool(Destructive = false, Idempotent = false, Name = "import-bills", ReadOnly = false, Title = "Import Bills")]
    [Description(
        "Records one or more utility bills, typically read from a supplier's invoice. " +
        "Bills whose invoice number or issue date already exists on the account are rejected rather than duplicated, so re-reading the same invoice is safe. " +
        "Each bill is accepted or rejected on its own; the result reports how many of each and why. " +
        "Do not supply the bill's total cost or overall usage total (the computed Total) — MooBank calculates those from the readings, periods and discounts.")]
    public ValueTask<ImportResult> ImportBills(
        [Description("The bills to record.")] BillImport request,
        CancellationToken cancellationToken = default) =>
        commandDispatcher.Dispatch(new Commands.Bills.Import(request.Bills.ToImportBill()), cancellationToken);
}
