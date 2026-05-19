using System.ComponentModel;
using Asm.MooBank.Modules.Reports.Models;
using Asm.MooBank.Modules.Reports.Queries;
using ModelContextProtocol.Server;

namespace Asm.MooBank.Modules.Reports.McpTools;

[McpServerToolType]
public class ReportTools(IQueryDispatcher queryDispatcher)
{
    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-cash-flow", ReadOnly = true, Title = "Get Cash Flow")]
    [Description("Returns total income, total outgoings and net cash flow across the user's transactional accounts (bank accounts and credit cards) for the supplied date range. If no range is supplied, defaults to the previous calendar month because imported transaction data is typically a month behind. This is the right tool for \"how much did I save?\" questions in the cash-flow sense.")]
    public ValueTask<UserCashFlowReport> GetCashFlow([Description("Optional inclusive date range. Omit to use the previous calendar month.")] GetUserCashFlow criteria)
    {
        return queryDispatcher.Dispatch(criteria);
    }

    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-spending-by-tag", ReadOnly = true, Title = "Get Spending By Tag")]
    [Description("Aggregates spending across the user's transactional accounts grouped by tag for the supplied date range. Use this for questions like \"how much did I spend on coffee last month?\". Pass parentTagId to scope the breakdown to children of a specific tag; omit to include all tags plus an Untagged bucket. Returns gross amount per tag, sorted descending.")]
    public ValueTask<UserSpendingByTagReport> GetSpendingByTag([Description("Optional inclusive date range and parent tag filter.")] GetUserSpendingByTag criteria)
    {
        return queryDispatcher.Dispatch(criteria);
    }

    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-spending-trend", ReadOnly = true, Title = "Get Spending Trend")]
    [Description("Returns a monthly series of income and outgoings across the user's transactional accounts for the supplied date range. Use for \"are we spending more than we earn?\" or \"how is our spending trending?\" questions.")]
    public ValueTask<UserSpendingTrendReport> GetSpendingTrend([Description("Optional inclusive date range. Omit to use the previous calendar month.")] GetUserSpendingTrend criteria)
    {
        return queryDispatcher.Dispatch(criteria);
    }

    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-savings-breakdown", ReadOnly = true, Title = "Get Savings Breakdown")]
    [Description("Composite view of how the user's wealth changed over a period. Returns the cash-flow net on transactional accounts plus the balance delta on each non-transactional instrument (savings, mortgage, loan, superannuation, investment, broker accounts). Stocks and other assets are included with their current value only, because historical balance is not tracked. Use this for \"how much did we save/grow our wealth?\" questions when the user wants more than just cash flow.")]
    public ValueTask<UserSavingsBreakdownReport> GetSavingsBreakdown([Description("Optional inclusive date range. Omit to use the previous calendar month.")] GetUserSavingsBreakdown criteria)
    {
        return queryDispatcher.Dispatch(criteria);
    }
}
