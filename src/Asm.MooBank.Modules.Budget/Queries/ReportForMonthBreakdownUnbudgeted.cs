using Asm.MooBank.Domain;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budget.Models;
using Azure;

namespace Asm.MooBank.Modules.Budget.Queries;

public record ReportForMonthBreakdownUnbudgeted(short Year, short Month) : IQuery<BudgetReportByMonthBreakdown>;

internal class ReportForMonthBreakdownUnbudgetedHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, IQueryable<Domain.Entities.Transactions.Transaction> transactions, AccountHolder accountHolder) : IQueryHandler<ReportForMonthBreakdownUnbudgeted, BudgetReportByMonthBreakdown>
{
    public async ValueTask<BudgetReportByMonthBreakdown> Handle(ReportForMonthBreakdownUnbudgeted query, CancellationToken cancellationToken)
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

        var otherTags = transactionTags.Except(lineTags);//.Select(t => t.Id);

        BudgetReportByMonthBreakdown breakdown = new(
            otherTags.Select(tag =>
                    new BudgetReportValueTag(
                        Name: tag.Name,
                        BudgetedAmount: 0,
                        Actual: budgetTransactions.SelectMany(t => t.Splits).Where(s => s.Tags.Any(t => t.Id == tag.Id)).Sum(t => Math.Abs(t.NetAmount))
                    )).OrderByDescending(b => b.Actual)
        );

        return breakdown;
    }
}
