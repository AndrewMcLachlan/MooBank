using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Forecast.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Forecast.Models;
using Asm.MooBank.Modules.Forecast.Services;
using DomainEntities = Asm.MooBank.Domain.Entities.Forecast;
using DomainTransaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Forecast.Queries;

/// <summary>
/// The payments that could plausibly belong to a planned item, for the author to choose from.
/// </summary>
/// <remarks>
/// The tag cannot say which payment is the item's, but it is a good filter for which ones are worth
/// looking at: spending that carried the item's tag, around the item's own date. That is all this
/// is for.
///
/// A recurring item is offered a window around each of its occurrences rather than one window from
/// the first to the last. School fees due each February should show five payments over five years,
/// not five years of everything else the school was ever paid for.
/// </remarks>
[DisplayName("GetPaymentCandidates")]
public record GetPaymentCandidates(Guid PlanId, Guid ItemId) : IQuery<IEnumerable<PaymentCandidate>>;

internal class GetPaymentCandidatesHandler(
    IQueryable<DomainEntities.ForecastPlan> plans,
    IQueryable<DomainTransaction> transactions,
    User user) : IQueryHandler<GetPaymentCandidates, IEnumerable<PaymentCandidate>>
{
    /// <summary>
    /// How far either side of a planned item's date a payment is worth offering.
    /// </summary>
    private const int CandidateWindowMonths = 2;

    public async ValueTask<IEnumerable<PaymentCandidate>> Handle(GetPaymentCandidates query, CancellationToken cancellationToken)
    {
        var plan = await plans
            .Specify(new ForecastPlanDetailsSpecification())
            .SingleAsync(p => p.Id == query.PlanId && p.FamilyId == user.FamilyId, cancellationToken);

        var item = plan.PlannedItems.SingleOrDefault(i => i.Id == query.ItemId)
            ?? throw new NotFoundException("Planned item not found");

        if (item.TagId is not { } tagId) return [];

        var accountIds = plan.AccountScopeMode == AccountScopeMode.SelectedAccounts
            ? plan.Accounts.Select(a => a.InstrumentId).ToList()
            : [.. user.Accounts, .. user.SharedAccounts];

        var (from, to) = CandidateWindow(item, plan);

        // Payments already claimed by another item on the plan are not on offer; a payment belongs
        // to one item.
        var claimedElsewhere = plan.PlannedItems
            .Where(i => i.Id != item.Id)
            .SelectMany(i => i.Transactions.Select(t => t.TransactionId))
            .ToList();

        var linkedHere = item.Transactions.Select(t => t.TransactionId).ToHashSet();

        var direction = item.ItemType == PlannedItemType.Income ? TransactionType.Credit : TransactionType.Debit;

        var start = from.ToStartOfDay();
        var end = to.ToEndOfDay();

        var candidates = await transactions
            .Where(t => accountIds.Contains(t.AccountId) &&
                        t.TransactionType == direction &&
                        !claimedElsewhere.Contains(t.Id) &&
                        t.TransactionTime >= start &&
                        t.TransactionTime <= end &&
                        t.Splits.SelectMany(s => s.Tags).Any(tag => tag.Id == tagId))
            .Select(t => new
            {
                t.Id,
                t.AccountId,
                t.TransactionTime,
                t.Description,
                Amount = DomainTransaction.TransactionNetAmount(t.TransactionType, t.Id, t.Amount),
            })
            .OrderByDescending(t => t.TransactionTime)
            .ToListAsync(cancellationToken);

        // A recurring item recurs: its whole span is not one window but a window around each
        // occurrence. Offering the span end to end buries five yearly school fee payments in five
        // years of every other transaction that happens to carry the school's tag.
        var wanted = CandidateMonths(item, plan);

        return [.. candidates
            .Where(c => wanted is null || wanted.Contains(new DateOnly(c.TransactionTime.Year, c.TransactionTime.Month, 1)))
            .Select(c => new PaymentCandidate
        {
            TransactionId = c.Id,
            AccountId = c.AccountId,
            When = DateOnly.FromDateTime(c.TransactionTime),
            Description = c.Description,
            Amount = Math.Abs(c.Amount),
            IsLinked = linkedHere.Contains(c.Id),
        })];
    }

    /// <summary>
    /// The months worth offering for a recurring item: those near one of its occurrences. Null for
    /// anything else, whose single window already says everything.
    /// </summary>
    internal static HashSet<DateOnly>? CandidateMonths(DomainEntities.ForecastPlannedItem item, DomainEntities.ForecastPlan plan)
    {
        if (item.DateMode != PlannedItemDateMode.Schedule || item.Schedule is null) return null;

        var months = new HashSet<DateOnly>();

        foreach (var occurrence in PlannedItemExpander.GenerateScheduleOccurrences(item, plan.StartDate, plan.EndDate))
        {
            var month = new DateOnly(occurrence.Year, occurrence.Month, 1);

            for (var offset = -CandidateWindowMonths; offset <= CandidateWindowMonths; offset++)
            {
                months.Add(month.AddMonths(offset));
            }
        }

        return months;
    }

    private static (DateOnly From, DateOnly To) CandidateWindow(DomainEntities.ForecastPlannedItem item, DomainEntities.ForecastPlan plan) =>
        item.DateMode switch
        {
            PlannedItemDateMode.FixedDate when item.FixedDate is not null =>
                (item.FixedDate.FixedDate.AddMonths(-CandidateWindowMonths), item.FixedDate.FixedDate.AddMonths(CandidateWindowMonths)),

            PlannedItemDateMode.Schedule when item.Schedule is not null =>
                (item.Schedule.AnchorDate.AddMonths(-CandidateWindowMonths),
                 (item.Schedule.EndDate ?? plan.EndDate).AddMonths(CandidateWindowMonths)),

            _ => (plan.StartDate, plan.EndDate),
        };
}
