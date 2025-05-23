﻿using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Queries;

public record Report(short Year) : IQuery<BudgetReportByMonth>;

internal class ReportHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, IQueryable<Domain.Entities.Transactions.Transaction> transactions, User user) : IQueryHandler<Report, BudgetReportByMonth>
{
    public async ValueTask<BudgetReportByMonth> Handle(Report request, CancellationToken cancellationToken)
    {
        var budget = await budgets.Include(b => b.Lines).SingleOrDefaultAsync(b => b.FamilyId == user.FamilyId && b.Year == request.Year, cancellationToken) ?? throw new NotFoundException();

        var budgetAccounts = await accounts.Where(a => a.IncludeInBudget && user.Accounts.Contains(a.Id)).Select(a => a.Id).ToArrayAsync(cancellationToken);

        var budgetTransactions = await transactions.Where(t => budgetAccounts.Contains(t.AccountId) && t.TransactionType == TransactionType.Debit && !t.ExcludeFromReporting && t.TransactionTime.Year == request.Year).ToArrayAsync(cancellationToken);

        var months = budget.ToMonths();

        return new BudgetReportByMonth
        {
            Items = months.Select(m => new BudgetReportValueMonth(m.Expenses, Math.Abs(budgetTransactions.Where(t => t.TransactionTime.Month == m.Month).Sum(t => Transaction.TransactionNetAmount(t.TransactionType, t.Id, t.Amount))), m.Month))
        };
    }
}
