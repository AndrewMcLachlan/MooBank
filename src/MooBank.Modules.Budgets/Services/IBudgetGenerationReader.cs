namespace Asm.MooBank.Modules.Budgets.Services;

/// <summary>
/// Read gateway for the transaction-history analysis inputs the budget generation
/// algorithm consumes. Keeps the command handler free of injected <see cref="IQueryable{T}"/>s.
/// </summary>
internal interface IBudgetGenerationReader
{
    Task<HashSet<int>> GetExistingLineTagIds(Guid familyId, short year, CancellationToken cancellationToken = default);

    Task<HashSet<int>> GetBudgetCategoryTagIds(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetBudgetedTagIds(Guid familyId, CancellationToken cancellationToken = default);

    Task<Guid[]> GetBudgetAccountIds(IEnumerable<Guid> userAccounts, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BudgetTransactionRow>> GetTransactionRows(Guid[] budgetAccounts, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    Task<HashSet<int>> GetExcludedTagIds(CancellationToken cancellationToken = default);

    Task<Dictionary<int, List<int>>> GetTagAncestors(CancellationToken cancellationToken = default);

    Task<Models.Budget> GetGeneratedBudget(Guid familyId, short year, CancellationToken cancellationToken = default);
}
