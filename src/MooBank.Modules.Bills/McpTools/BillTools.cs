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

    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-bills", ReadOnly = true, Title = "Get Bills")]
    [Description(
        "Retrieves the utility bills already recorded, most recently issued first, as a page of bills plus the total number that matched. " +
        "Use it for questions about what has been billed and what was used: what the last electricity bill came to, how gas usage compares year on year, whether a bill for a given month has been recorded yet. " +
        "Each bill carries its invoice number, issue date, meter readings, billing periods (usage, price per unit, daily service charge), discounts, and the cost and total MooBank computed from them. " +
        "get-bill-accounts is the cheaper way to find out which accounts exist and which dates they already have bills for; come here for the bills themselves. " +
        "Filter by issue date range, by account id (from get-bill-accounts) or by utility type; only the current user's accounts are ever returned.")]
    public ValueTask<PagedResult<Models.Bill>> GetBills(
        [Description("Filter and paging criteria. Every part is optional; supply an empty object for the 20 most recently issued bills across all accounts.")] BillQuery criteria,
        CancellationToken cancellationToken = default) =>
        queryDispatcher.Dispatch(new Queries.Bills.GetAll
        {
            PageSize = criteria.PageSize,
            PageNumber = criteria.PageNumber,
            StartDate = criteria.StartDate,
            EndDate = criteria.EndDate,
            AccountId = criteria.AccountId,
            UtilityType = criteria.UtilityType,
        }, cancellationToken);

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
