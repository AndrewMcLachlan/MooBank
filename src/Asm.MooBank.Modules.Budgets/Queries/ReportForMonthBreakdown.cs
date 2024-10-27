using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;

namespace Asm.MooBank.Modules.Budgets.Queries;

/// <summary>
/// The budget report for a given year and month, broken down by tag.
/// Tags that are not in the budget are included in the "Other" category.
/// </summary>
/// <param name="Year">The budget year.</param>
/// <param name="Month">The budget month.</param>
public record ReportForMonthBreakdown(short Year, short Month) : IQuery<BudgetReportByMonthBreakdown>;

internal class ReportForMonthBreakdownHandler(IQueryable<Domain.Entities.Budget.Budget> budgets, IQueryable<Domain.Entities.Account.InstitutionAccount> accounts, IQueryable<Domain.Entities.Transactions.Transaction> transactions, IQueryable<TagRelationship> tagRelationships, User user) : IQueryHandler<ReportForMonthBreakdown, BudgetReportByMonthBreakdown>
{
    public async ValueTask<BudgetReportByMonthBreakdown> Handle(ReportForMonthBreakdown query, CancellationToken cancellationToken)
    {
        var budget = await budgets.Include(b => b.Lines).ThenInclude(l => l.Tag).ThenInclude(t => t.Settings).SingleOrDefaultAsync(b => b.FamilyId == user.FamilyId && b.Year == query.Year, cancellationToken) ?? throw new NotFoundException();

        var budgetAccounts = await accounts.Where(a => a.IncludeInBudget && user.Accounts.Contains(a.Id)).Select(a => a.Id).ToArrayAsync(cancellationToken);

        // Get expense transactions for the given year and month.
        var budgetTransactions = await transactions.Specify(new IncludeSplitsSpecification()).Where(t =>
                budgetAccounts.Contains(t.AccountId) &&
                TransactionTypes.Debit.Contains(t.TransactionType) &&
                !t.ExcludeFromReporting &&
                t.TransactionTime.Year == query.Year && t.TransactionTime.Month == query.Month
            ).ToArrayAsync(cancellationToken);

        // Get all tags used in the budget transactions
        var transactionTags = budgetTransactions.SelectMany(t => t.Splits.SelectMany(ts => ts.Tags.IncludedInReporting())).Distinct();

        // Get all expense lines for the month
        var lines = budget.Lines.WhereMonth(query.Month).Where(l => !l.Income);

        var lineTags = lines.Select(l => l.Tag);

        // Get tag relationships and convert them to a hierarchy
        // TODO: Can this be made more generic?
        var relationships = await tagRelationships.ToListAsync(cancellationToken);
        IEnumerable<TagHierarchy> lineTagHierarchy = lineTags.ToHierarchy(relationships);

        // Get any transaction tags that are not in the line tag hierarchy as a descendent
        var otherTagIds = transactionTags.Where(t => !lineTagHierarchy.Any(lt => lt.Id == t.Id) && !lineTagHierarchy.SelectMany(lt => lt.Descendants).Contains(t.Id)).Select(t => t.Id);

        BudgetReportByMonthBreakdown breakdown = new(
            lineTagHierarchy.Select(tag =>
                    new BudgetReportValueTag(tag.Name,
                        lines.Where(t => t.TagId == tag.Id).Sum(l => l.Amount),
                        budgetTransactions.Where(s => s.Tags.Any(t => t.Id == tag.Id) || tag.Descendants.Intersect(s.Tags.Select(t => t.Id)).Any()).Sum(t => Math.Abs(Transaction.TransactionNetAmount(t.Id, t.Amount)))
                    )).OrderByDescending(b => b.BudgetedAmount)
                    .ThenByDescending(b => b.Actual)
                    .Append(new(
                        Name: "Other",
                        BudgetedAmount: 0,
                        Actual: budgetTransactions.Where(s => s.Tags.Any(t => otherTagIds.Contains(t.Id))).Sum(t => Math.Abs(Transaction.TransactionNetAmount(t.Id, t.Amount)))
                    ))
        );

        return breakdown;
    }
}

/// <summary>
/// A tag and it's descendants and ancestors (flattened).
/// </summary>
/// <remarks>
/// The relationships here include all, not just direct.
/// </remarks>
public class TagHierarchy : Domain.Entities.Tag.Tag
{
    public TagHierarchy(Domain.Entities.Tag.Tag tag) : base(tag.Id)
    {
        Name = tag.Name;
    }

    public required int[] Ancestors { get; set; }

    public required int[] Descendants { get; set; }
}

public static class TagExtensions
{
    /// <summary>
    /// Create the flattened structure.
    /// </summary>
    /// <param name="tags">The tags to enhance.</param>
    /// <param name="relationships">The relationship information.</param>
    /// <returns>The <paramref name="tags"/> with ancestors and descendants.</returns>
    public static IEnumerable<TagHierarchy> ToHierarchy(this IEnumerable<Domain.Entities.Tag.Tag> tags, IEnumerable<TagRelationship> relationships)
    {
        foreach (var tag in tags)
        {
            yield return new TagHierarchy(tag)
            {
                Ancestors = relationships.Where(r => r.Id == tag.Id).Select(r => r.ParentId).ToArray(),
                Descendants = relationships.Where(r => r.ParentId == tag.Id).Select(r => r.Id).ToArray()
            };
        }
    }
}
