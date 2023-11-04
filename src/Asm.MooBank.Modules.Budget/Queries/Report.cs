using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budget.Models;

namespace Asm.MooBank.Modules.Budget.Queries;

public record Report(short Year) : IQuery<BudgetReportByMonth>;

internal class ReportHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, IQueryable<Domain.Entities.Transactions.Transaction> transactions, AccountHolder accountHolder) : IQueryHandler<Report, BudgetReportByMonth>
{
    private readonly IQueryable<Domain.Entities.Budget.Budget> _budgets = budgets;
    private readonly IQueryable<Domain.Entities.Account.InstitutionAccount> _accounts = accounts;
    private readonly IQueryable<Domain.Entities.Transactions.Transaction> _transactions = transactions;
    private readonly AccountHolder _accountHolder = accountHolder;

    public async ValueTask<BudgetReportByMonth> Handle(Report request, CancellationToken cancellationToken)
    {
        var budget = await _budgets.Include(b => b.Lines).SingleOrDefaultAsync(b => b.FamilyId == _accountHolder.FamilyId && b.Year == request.Year, cancellationToken) ?? throw new NotFoundException();

        var accounts = await _accounts.Where(a => a.IncludeInBudget && _accountHolder.Accounts.Contains(a.AccountId)).Select(a => a.AccountId).ToArrayAsync(cancellationToken);

        var transactions = await _transactions.Where(t => accounts.Contains(t.AccountId) && TransactionTypes.Debit.Contains(t.TransactionType) && !t.ExcludeFromReporting && t.TransactionTime.Year == request.Year).ToArrayAsync(cancellationToken);

        var months = budget.ToMonths();

        return new BudgetReportByMonth
        {
            Items = months.Select(m => new BudgetReportValueMonth(m.Expenses, Math.Abs(transactions.Where(t => t.TransactionTime.Month == m.Month + 1).Sum(t => t.NetAmount)), m.Month))
        };
    }
}

public record BudgetReportByMonth
{
    public IEnumerable<BudgetReportValueMonth> Items { get; init; } = Enumerable.Empty<BudgetReportValueMonth>();
}

public record BudgetReportValue(decimal BudgetedAmount, decimal? Actual);

public record BudgetReportValueMonth(decimal BudgetedAmount, decimal? Actual, int Month) : BudgetReportValue(BudgetedAmount, Actual);