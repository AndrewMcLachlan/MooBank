using System.ComponentModel;
using Asm.MooBank.Modules.Forecast.Models;
using ModelContextProtocol.Server;

namespace Asm.MooBank.Modules.Forecast.McpTools;

[McpServerToolType]
public class ForecastTools(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher)
{
    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-forecast-plans", ReadOnly = true, Title = "Get Forecast Plans")]
    [Description(
        "Lists the household's cash-flow forecast plans, most recently updated first: each plan's name, date range, account scope mode, how its starting balance is set, its currency and its outgoing strategy and assumptions. " +
        "The accounts a plan covers and its planned items (the expected income and one-off or recurring expenses) come back empty here; get them for one plan with get-forecast-plan. " +
        "Archived plans are left out unless asked for.")]
    public ValueTask<IEnumerable<ForecastPlan>> GetPlans(
        [Description("Include archived plans. Defaults to false.")] bool includeArchived = false,
        CancellationToken cancellationToken = default) =>
        queryDispatcher.Dispatch(new Queries.GetPlans(includeArchived), cancellationToken);

    [McpServerTool(Destructive = false, Idempotent = true, Name = "get-forecast-plan", ReadOnly = true, Title = "Get Forecast Plan")]
    [Description(
        "Retrieves one forecast plan in full, including the accounts it covers and its planned items: the income and expenses the author expects, with their amounts, schedules and tags. " +
        "This is the inputs, not the answer. Run the forecast itself with run-forecast.")]
    public ValueTask<ForecastPlan> GetPlan(
        [Description("The plan to retrieve, from get-forecast-plans.")] Guid planId,
        CancellationToken cancellationToken = default) =>
        queryDispatcher.Dispatch(new Queries.GetPlan(planId), cancellationToken);

    [McpServerTool(Destructive = false, Idempotent = true, Name = "run-forecast", ReadOnly = true, Title = "Run Forecast")]
    [Description(
        "Runs a forecast plan month by month across its date range: each month's opening and closing balance, income, baseline outgoings and planned expenses, with the actual balance, income and outgoings for months that have already happened. " +
        "The summary gives the lowest balance and when it falls, how many months go below zero, the monthly uplift needed to stay above zero, total income and outgoings, and the fitted expense model (a fixed monthly amount plus a share of income, with its R² and the shortfall between modelled and actual income). " +
        "Planned-item progress shows each item's planned total against what has actually been spent on its tag so far. " +
        "Nothing is saved, so this is safe to run repeatedly.")]
    public ValueTask<ForecastResult> RunForecast(
        [Description("The plan to run, from get-forecast-plans.")] Guid planId,
        CancellationToken cancellationToken = default) =>
        commandDispatcher.Dispatch(new Commands.RunForecast(planId), cancellationToken);
}
