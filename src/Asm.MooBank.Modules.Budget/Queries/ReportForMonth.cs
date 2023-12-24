using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budget.Models;

namespace Asm.MooBank.Modules.Budget.Queries;

public record ReportForMonth(short Year, short Month) : IQuery<BudgetReportValueMonth?>;

internal class ReportForMonthHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, IQueryable<Domain.Entities.Transactions.Transaction> transactions, AccountHolder accountHolder) : IQueryHandler<ReportForMonth, BudgetReportValueMonth?>
{
    public async ValueTask<BudgetReportValueMonth?> Handle(ReportForMonth request, CancellationToken cancellationToken)
    {
        var budget = await budgets.Include(b => b.Lines).SingleOrDefaultAsync(b => b.FamilyId == accountHolder.FamilyId && b.Year == request.Year, cancellationToken) ?? throw new NotFoundException();

        var budgetAccounts = await accounts.Where(a => a.IncludeInBudget && accountHolder.Accounts.Contains(a.Id)).Select(a => a.Id).ToArrayAsync(cancellationToken);

        var budgetTransactions = await transactions.Where(t => budgetAccounts.Contains(t.AccountId) && TransactionTypes.Debit.Contains(t.TransactionType) && !t.ExcludeFromReporting && t.TransactionTime.Year == request.Year).ToArrayAsync(cancellationToken);

        var month = budget.ToMonths()
            .Where(m => m.Month == request.Month)
            .Select(m => new BudgetReportValueMonth(m.Expenses, Math.Abs(budgetTransactions.Where(t => t.TransactionTime.Month == m.Month + 1).Sum(t => t.NetAmount)), m.Month))
            .SingleOrDefault();

        return month;
    }
}
