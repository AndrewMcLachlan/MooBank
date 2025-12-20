using System.ComponentModel;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Forecast.Models;

[DisplayName("ForecastPlan")]
public sealed record ForecastPlan
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public AccountScopeMode AccountScopeMode { get; init; }
    public StartingBalanceMode StartingBalanceMode { get; init; }
    public decimal? StartingBalanceAmount { get; init; }
    public string? CurrencyCode { get; init; }
    public IncomeStrategy? IncomeStrategy { get; init; }
    public OutgoingStrategy? OutgoingStrategy { get; init; }
    public Assumptions? Assumptions { get; init; }
    public bool IsArchived { get; init; }
    public DateTime CreatedUtc { get; init; }
    public DateTime UpdatedUtc { get; init; }
    public IEnumerable<Guid> AccountIds { get; init; } = [];
    public IEnumerable<PlannedItem> PlannedItems { get; init; } = [];
}
