using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Queries;

public record ReportForMonth(short Year, short Month) : IQuery<BudgetReportValueMonth?>;

internal class ReportForMonthHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, IQueryable<Domain.Entities.Account.LogicalAccount> accounts, IQueryable<Domain.Entities.Transactions.Transaction> transactions, User user) : IQueryHandler<ReportForMonth, BudgetReportValueMonth?>
{
    public async ValueTask<BudgetReportValueMonth?> Handle(ReportForMonth request, CancellationToken cancellationToken)
    {
        var budget = await budgets.Include(b => b.Lines).SingleOrDefaultAsync(b => b.FamilyId == user.FamilyId && b.Year == request.Year, cancellationToken) ?? throw new NotFoundException();

        var budgetAccounts = await accounts.Where(a => a.IncludeInBudget && user.Accounts.Contains(a.Id)).Select(a => a.Id).ToArrayAsync(cancellationToken);

        var monthTotal = await transactions
            .Where(t => budgetAccounts.Contains(t.AccountId) &&
                        t.TransactionType == TransactionType.Debit &&
                        !t.ExcludeFromReporting &&
                        t.TransactionTime.Year == request.Year &&
                        t.TransactionTime.Month == request.Month)
            .SumAsync(t => Domain.Entities.Transactions.Transaction.TransactionNetAmount(t.TransactionType, t.Id, t.Amount), cancellationToken);

        var month = budget.ToMonths()
            .Where(m => m.Month == request.Month)
            .Select(m => new BudgetReportValueMonth(m.Expenses, Math.Abs(monthTotal), m.Month))
            .SingleOrDefault();

        return month;
    }
}
