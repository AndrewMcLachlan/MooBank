using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Reports.Models;

public record UserSavingsBreakdownReport
{
    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public required UserCashFlowReport CashFlow { get; init; }

    public required IEnumerable<InstrumentBalanceChange> InstrumentChanges { get; init; } = [];

    public decimal TotalWealthChange => CashFlow.Net + InstrumentChanges.Sum(c => c.Delta ?? 0);
}

public record InstrumentBalanceChange
{
    public required Guid InstrumentId { get; init; }

    public required string Name { get; init; }

    public required AccountType? AccountType { get; init; }

    public decimal? StartBalance { get; init; }

    public decimal? EndBalance { get; init; }

    public decimal? Delta { get; init; }

    public string? Note { get; init; }
}
