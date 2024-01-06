using Asm.MooBank.Domain;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budget.Models;
using Azure;

namespace Asm.MooBank.Modules.Budget.Queries;

public record ReportForMonthBreakdown(short Year, short Month) : IQuery<BudgetReportByMonthBreakdown>;

internal class ReportForMonthBreakdownHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, IQueryable<Domain.Entities.Transactions.Transaction> transactions, AccountHolder accountHolder) : IQueryHandler<ReportForMonthBreakdown, BudgetReportByMonthBreakdown>
{
    public async ValueTask<BudgetReportByMonthBreakdown> Handle(ReportForMonthBreakdown query, CancellationToken cancellationToken)
    {
        var budget = await budgets.Include(b => b.Lines).ThenInclude(l => l.Tag).ThenInclude(t => t.Settings).SingleOrDefaultAsync(b => b.FamilyId == accountHolder.FamilyId && b.Year == query.Year, cancellationToken) ?? throw new NotFoundException();

        var budgetAccounts = await accounts.Where(a => a.IncludeInBudget && accountHolder.Accounts.Contains(a.Id)).Select(a => a.Id).ToArrayAsync(cancellationToken);

        var budgetTransactions = await transactions.Specify(new IncludeSplitsSpecification()).Where(t =>
                budgetAccounts.Contains(t.AccountId) &&
                TransactionTypes.Debit.Contains(t.TransactionType) &&
                !t.ExcludeFromReporting &&
                t.TransactionTime.Year == query.Year && t.TransactionTime.Month == query.Month
            ).ToArrayAsync(cancellationToken);

        var transactionTags = budgetTransactions.SelectMany(t => t.Splits.SelectMany(ts => ts.Tags.Where(tg => !tg.Settings.ExcludeFromReporting))).Distinct();

        var lines = budget.Lines.WhereMonth(query.Month).Where(l => !l.Income);

        var lineTags = lines.Select(l => l.Tag);

        //var allTags = transactionTags.Union(lineTags).Distinct(new TagEqualityComparer());
        var otherTagIds = transactionTags.Except(lineTags).Select(t => t.Id);

        // TODO: Setup net amount for splits.
        BudgetReportByMonthBreakdown breakdown = new(
            lineTags.Select(tag =>
                    new BudgetReportValueTag(tag.Name,
                        lines.Where(t => t.TagId == tag.Id).Sum(l => l.Amount),
                        budgetTransactions.SelectMany(t => t.Splits).Where(s => s.Tags.Any(t => t.Id == tag.Id)).Sum(t => Math.Abs(t.NetAmount))
                    )).OrderByDescending(b => b.BudgetedAmount)
                    .ThenByDescending(b => b.Actual)
                    .Append(new(
                        Name: "Other",
                        BudgetedAmount: 0,
                        Actual: budgetTransactions.SelectMany(t => t.Splits).Where(s => s.Tags.Any(t => otherTagIds.Contains(t.Id))).Sum(t => Math.Abs(t.NetAmount))
                    ))
        );

            //.Select(m => new BudgetReportValueMonth(m.Expenses, Math.Abs(budgetTransactions.Where(t => t.TransactionTime.Month == m.Month + 1).Sum(t => t.NetAmount)), m.Month))
            //.SingleOrDefault();

        return breakdown;
    }
}
