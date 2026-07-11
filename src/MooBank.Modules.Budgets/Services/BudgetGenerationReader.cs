using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Budget.Specifications;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;
using DomainBudget = Asm.MooBank.Domain.Entities.Budget.Budget;
using DomainTag = Asm.MooBank.Domain.Entities.Tag.Tag;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Budgets.Services;

internal class BudgetGenerationReader(
    IQueryable<DomainBudget> budgets,
    IQueryable<LogicalAccount> accounts,
    IQueryable<Transaction> transactions,
    IQueryable<DomainTag> tags,
    IQueryable<TagRelationship> tagRelationships) : IBudgetGenerationReader
{
    public async Task<HashSet<int>> GetExistingLineTagIds(Guid familyId, short year, CancellationToken cancellationToken = default)
    {
        var existingBudget = await budgets
            .Specify(new BudgetWithLinesSpecification())
            .SingleOrDefaultAsync(b => b.FamilyId == familyId && b.Year == year, cancellationToken);

        return existingBudget?.Lines.Select(l => l.TagId).ToHashSet() ?? [];
    }

    public async Task<HashSet<int>> GetBudgetCategoryTagIds(CancellationToken cancellationToken = default) =>
        (await tags
            .Where(t => t.Settings.BudgetCategory)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken)).ToHashSet();

    public async Task<IReadOnlyList<int>> GetBudgetedTagIds(Guid familyId, CancellationToken cancellationToken = default) =>
        await budgets
            .Where(b => b.FamilyId == familyId)
            .SelectMany(b => b.Lines)
            .Select(l => l.TagId)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<Guid[]> GetBudgetAccountIds(IEnumerable<Guid> userAccounts, CancellationToken cancellationToken = default) =>
        await accounts
            .Where(a => a.IncludeInBudget && userAccounts.Contains(a.Id))
            .Select(a => a.Id)
            .ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<BudgetTransactionRow>> GetTransactionRows(Guid[] budgetAccounts, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        var rows = await transactions
            .Where(t => budgetAccounts.Contains(t.AccountId) && !t.ExcludeFromReporting &&
                        t.TransactionTime >= startTime && t.TransactionTime < endTime &&
                        Transaction.TransactionNetAmount(t.TransactionType, t.Id, t.Amount) != 0m)
            .SelectMany(t => t.Splits.SelectMany(s => s.Tags.Select(tag => new
            {
                t.TransactionTime.Year,
                t.TransactionTime.Month,
                TagId = tag.Id,
                Net = (t.TransactionType == TransactionType.Credit ? 1m : -1m) * (s.Amount - s.OffsetBy.Sum(o => o.Amount)),
            })))
            .ToListAsync(cancellationToken);

        return rows.Select(r => new BudgetTransactionRow(r.Year, r.Month, r.TagId, r.Net)).ToList();
    }

    public async Task<HashSet<int>> GetExcludedTagIds(CancellationToken cancellationToken = default) =>
        (await tags
            .Where(t => t.Settings.ExcludeFromReporting)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken)).ToHashSet();

    public async Task<Dictionary<int, List<int>>> GetTagAncestors(CancellationToken cancellationToken = default) =>
        (await tagRelationships
                .Select(r => new { r.Id, r.ParentId, r.Ordinal })
                .ToListAsync(cancellationToken))
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Ordinal).Select(x => x.ParentId).ToList());

    public async Task<Models.Budget> GetGeneratedBudget(Guid familyId, short year, CancellationToken cancellationToken = default)
    {
        var result = await budgets
            .Include(b => b.Lines).ThenInclude(l => l.Tag)
            .IgnoreQueryFilters(["SoftDelete"])
            .SingleAsync(b => b.FamilyId == familyId && b.Year == year, cancellationToken);

        return result.ToModel();
    }
}
