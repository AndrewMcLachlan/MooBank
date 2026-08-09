using Asm.MooBank.Models;
using DomainForecastPlan = Asm.MooBank.Domain.Entities.Forecast.ForecastPlan;

namespace Asm.MooBank.Modules.Forecast.Services;

/// <summary>
/// The accounts a plan is allowed to see.
/// </summary>
/// <remarks>
/// Shared rather than worked out at each call site, because the two places that needed it had
/// drifted apart: the candidate list filtered by these accounts while the command that recorded the
/// author's answer filtered by nothing at all, so a payment that could never have been offered
/// could still be linked.
/// </remarks>
internal static class PlanScope
{
    public static IReadOnlyList<Guid> AccountIds(DomainForecastPlan plan, User user) =>
        plan.AccountScopeMode == AccountScopeMode.SelectedAccounts
            ? [.. plan.Accounts.Select(a => a.InstrumentId)]
            : [.. user.Accounts, .. user.SharedAccounts];
}
