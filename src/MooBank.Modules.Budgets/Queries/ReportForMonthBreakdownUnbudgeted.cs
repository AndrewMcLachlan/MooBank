using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Queries;

public record ReportForMonthBreakdownUnbudgeted(short Year, short Month) : IQuery<BudgetReportByMonthBreakdown>;

internal class ReportForMonthBreakdownUnbudgetedHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, IQueryable<Domain.Entities.Account.LogicalAccount> accounts, IQueryable<Domain.Entities.Transactions.Transaction> transactions, IQueryable<TagRelationship> tagRelationships, User user) : IQueryHandler<ReportForMonthBreakdownUnbudgeted, BudgetReportByMonthBreakdown>
{
    public async ValueTask<BudgetReportByMonthBreakdown> Handle(ReportForMonthBreakdownUnbudgeted query, CancellationToken cancellationToken)
    {
        var budget = await budgets.Include(b => b.Lines).ThenInclude(l => l.Tag).ThenInclude(t => t.Settings).IgnoreQueryFilters(["SoftDelete"]).SingleOrDefaultAsync(b => b.FamilyId == user.FamilyId && b.Year == query.Year, cancellationToken) ?? throw new NotFoundException();

        var budgetAccounts = await accounts.Where(a => a.IncludeInBudget && user.Accounts.Contains(a.Id)).Select(a => a.Id).ToArrayAsync(cancellationToken);

        var budgetTransactions = await transactions.Specify(new IncludeSplitsSpecification()).IgnoreQueryFilters(["SoftDelete"]).Where(t =>
                budgetAccounts.Contains(t.AccountId) &&
                t.TransactionType == TransactionType.Debit &&
                !t.ExcludeFromReporting &&
                t.TransactionTime.Year == query.Year && t.TransactionTime.Month == query.Month
            ).ToArrayAsync(cancellationToken);

        var transactionTags = budgetTransactions.SelectMany(t => t.Splits.SelectMany(ts => ts.Tags.IncludedInReporting())).Distinct();

        var lines = budget.Lines.WhereMonth(query.Month).Where(l => !l.Income);

        var lineTags = lines.Select(l => l.Tag);

        var otherTags = transactionTags.ExceptWhereRelationship(lineTags, await tagRelationships.ToListAsync(cancellationToken)).ToList();

        var splits = budgetTransactions.SelectMany(t => t.Splits).ToArray();

        BudgetReportByMonthBreakdown breakdown = new(
            otherTags.Select(tag =>
                    new BudgetReportValueTag(
                        Name: tag.Name,
                        BudgetedAmount: 0,
                        Actual: splits.Where(s => s.Tags.Any(t => t.Id == tag.Id)).Sum(s => s.GetNetAmount())
                    )).OrderByDescending(b => b.Actual)
        );

        return breakdown;
    }
}

public static class Extensions
{
    public static IEnumerable<Domain.Entities.Tag.Tag> ExceptWhereRelationship(this IEnumerable<Domain.Entities.Tag.Tag> tags, IEnumerable<Domain.Entities.Tag.Tag> second, IEnumerable<TagRelationship> tagRelationships)
    {
        var secondIds = second.Select(s => s.Id).ToHashSet();

        foreach (var tag in tags)
        {
            if (secondIds.Contains(tag.Id)) continue;

            var isChildOfSecond = tagRelationships.Any(r => r.Id == tag.Id && secondIds.Contains(r.ParentId));

            if (!isChildOfSecond)
            {
                yield return tag;
            }
        }
    }
}
