using Asm.MooBank.Models;

namespace Asm.MooBank.Queries.Budget;

public record Report(short Year) : IQuery<BudgetReportByMonth>;

internal class ReportHandler : IQueryHandler<Report, BudgetReportByMonth>
{
    private readonly IQueryable<Domain.Entities.Budget.Budget> _budgets;
    private readonly IQueryable<Domain.Entities.Account.InstitutionAccount> _accounts;
    private readonly IQueryable<Domain.Entities.Transactions.Transaction> _transactions;
    private readonly Models.AccountHolder _accountHolder;

    public ReportHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, IQueryable<Domain.Entities.Transactions.Transaction> transactions, Models.AccountHolder accountHolder)
    {
        _budgets = budgets;
        _accounts = accounts;
        _transactions = transactions;
        _accountHolder = accountHolder;
    }

    public async Task<BudgetReportByMonth> Handle(Report request, CancellationToken cancellationToken)
    {
        var budget = await _budgets.Include(b => b.Lines).SingleOrDefaultAsync(b => b.FamilyId == _accountHolder.FamilyId && b.Year == request.Year, cancellationToken) ?? throw new NotFoundException();

        var accounts = await _accounts.Where(a => a.IncludeInBudget && _accountHolder.Accounts.Contains(a.AccountId)).Select(a => a.AccountId).ToArrayAsync(cancellationToken);

        var transactions = await _transactions.Where(t => accounts.Contains(t.AccountId) && TransactionTypes.Debit.Contains(t.TransactionType) && !t.ExcludeFromReporting && t.TransactionTime.Year == request.Year).ToArrayAsync(cancellationToken);

        var months = budget.ToMonths();

        return new BudgetReportByMonth
        {
            Items = months.Select(m => new BudgetReportValueMonth(m.Expenses, Math.Abs(transactions.Where(t => t.TransactionTime.Month == (m.Month+1)).Sum(t => t.NetAmount)), m.Month))
        };
    }
}

public record BudgetReportByMonth
{
    public IEnumerable<BudgetReportValueMonth> Items { get; init; } = Enumerable.Empty<BudgetReportValueMonth>();
}

public record BudgetReportValue(decimal BudgetedAmount, decimal? Actual);

public record BudgetReportValueMonth(decimal BudgetedAmount, decimal? Actual, int Month) : BudgetReportValue(BudgetedAmount, Actual);