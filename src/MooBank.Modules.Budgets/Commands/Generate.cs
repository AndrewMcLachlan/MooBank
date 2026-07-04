using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Budgets.Models;
using Asm.MooBank.Modules.Budgets.Services;
using DomainBudgetLine = Asm.MooBank.Domain.Entities.Budget.BudgetLine;
using Transaction = Asm.MooBank.Domain.Entities.Transactions.Transaction;

namespace Asm.MooBank.Modules.Budgets.Commands;

public record GenerateBudget(short Year) : ICommand<Models.Budget>;

/// <summary>
/// Builds budget lines for a year by analysing the family's recent transaction
/// history. Detects periodic payments (yearly / quarterly / monthly) and rolls
/// lumpy spend up into an averaged monthly amount. Spend is rolled up to the tag
/// level the family actually budgets at, so suggestions aren't overly granular.
/// Existing lines are left untouched — only tags that aren't yet budgeted for the
/// year get a line.
/// </summary>
internal class GenerateBudgetHandler(
    IQueryable<Domain.Entities.Budget.Budget> budgets,
    IQueryable<LogicalAccount> accounts,
    IQueryable<Transaction> transactions,
    IQueryable<Domain.Entities.Tag.Tag> tags,
    IQueryable<TagRelationship> tagRelationships,
    IBudgetRepository budgetRepository,
    IUnitOfWork unitOfWork,
    User user) : ICommandHandler<GenerateBudget, Models.Budget>
{
    private const int WindowMonths = 24;

    public async ValueTask<Models.Budget> Handle(GenerateBudget request, CancellationToken cancellationToken)
    {
        // Security: family-scoped via the user; year is the only input (as with CreateLine).
        var budget = await budgetRepository.GetOrCreate(user.FamilyId, request.Year, cancellationToken);

        var existingBudget = await budgets
            .Include(b => b.Lines)
            .SingleOrDefaultAsync(b => b.FamilyId == user.FamilyId && b.Year == request.Year, cancellationToken);
        var existingTagIds = existingBudget?.Lines.Select(l => l.TagId).ToHashSet() ?? [];

        // Rollup targets: the levels at which budget lines are generated. Spend rolls up to
        // the nearest target ancestor. A tag is a target if it's marked as a budget category,
        // OR if the family has budgeted it before — so areas without an explicit category
        // still roll up to the level they're used to (as before tag settings existed), and
        // marking a tag simply adds or forces a rollup level.
        var rollupTargets = (await tags
            .Where(t => t.Settings.BudgetCategory)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken)).ToHashSet();

        var budgetedTagIds = await budgets
            .Where(b => b.FamilyId == user.FamilyId)
            .SelectMany(b => b.Lines)
            .Select(l => l.TagId)
            .Distinct()
            .ToListAsync(cancellationToken);
        rollupTargets.UnionWith(budgetedTagIds);

        var budgetAccounts = await accounts
            .Where(a => a.IncludeInBudget && user.Accounts.Contains(a.Id))
            .Select(a => a.Id)
            .ToArrayAsync(cancellationToken);

        if (budgetAccounts.Length != 0)
        {
            var end = DateTime.Today.ToDateOnly().ToStartOfMonth();   // first day of the current (incomplete) month
            var start = end.AddMonths(-WindowMonths);

            var startTime = start.ToStartOfDay();
            var endTime = end.ToStartOfDay();

            // Flatten to one row per split-tag, attributing a signed NET amount to the
            // split's directly-applied tags. Positive = money in (income), negative =
            // money out (expense). Transactions whose net is zero — fully offset, e.g. a
            // refund linked to its purchase, or a transfer that nets out — are dropped
            // entirely (mirrors the report's net-of-offsets via TransactionNetAmount, the
            // same DB function Report.cs uses), so offsetting entries don't inflate the
            // budget. Split-level offsets reduce the split they apply to.
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

            var excludedTagIds = await tags
                .Where(t => t.Settings.ExcludeFromReporting)
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);
            var excluded = excludedTagIds.ToHashSet();

            // Ancestors of each tag, from the TagHierarchies closure view: a row's Id is the
            // tag and ParentId is an ancestor (every ancestor is listed, not just the direct
            // parent). Ordinal runs topmost-first, so order ancestors nearest-first (highest
            // ordinal) to find the closest budget-category ancestor.
            var ancestorsOf = (await tagRelationships
                    .Select(r => new { r.Id, r.ParentId, r.Ordinal })
                    .ToListAsync(cancellationToken))
                .GroupBy(r => r.Id)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Ordinal).Select(x => x.ParentId).ToList());

            // Roll a tag up to the nearest ancestor (including the tag itself) that is a
            // rollup target. If neither the tag nor any ancestor is a target, keep it as
            // applied rather than rolling it up — so distinct categories aren't merged.
            int RollUp(int tagId)
            {
                if (rollupTargets.Contains(tagId)) return tagId;
                if (ancestorsOf.TryGetValue(tagId, out var ancestors))
                {
                    foreach (var ancestor in ancestors)
                        if (rollupTargets.Contains(ancestor)) return ancestor;
                }
                return tagId;
            }

            var candidates = rows
                .Where(r => !excluded.Contains(r.TagId))
                .Select(r => new { TagId = RollUp(r.TagId), r.Year, r.Month, r.Net })
                .Where(r => !existingTagIds.Contains(r.TagId))
                .GroupBy(r => r.TagId);

            var added = false;
            foreach (var candidate in candidates)
            {
                var monthlyNet = candidate
                    .GroupBy(r => (r.Year, r.Month))
                    .Select(g => new { g.Key.Year, g.Key.Month, Net = g.Sum(x => x.Net) })
                    .ToList();

                // A tag is income or expense based on its overall net flow over the window.
                var totalNet = monthlyNet.Sum(m => m.Net);
                if (totalNet == 0m) continue;

                var income = totalNet > 0m;
                var sign = income ? 1m : -1m;

                var series = monthlyNet
                    .Select(m => new MonthlySpend(m.Year, m.Month, Math.Max(0m, sign * m.Net)))
                    .ToList();

                var suggestion = BudgetSuggestionCalculator.Calculate(series);
                if (suggestion is null || suggestion.Amount <= 0m) continue;

                budgetRepository.AddLine(new DomainBudgetLine(Guid.NewGuid())
                {
                    BudgetId = budget.Id,
                    TagId = candidate.Key,
                    Income = income,
                    Amount = suggestion.Amount,
                    Month = suggestion.Months,
                    Notes = suggestion.Note,
                });
                added = true;
            }

            if (added) await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Re-read so the returned model reflects every line (existing + generated) with tags.
        var result = await budgets
            .Include(b => b.Lines).ThenInclude(l => l.Tag)
            .SingleAsync(b => b.FamilyId == user.FamilyId && b.Year == request.Year, cancellationToken);

        return result.ToModel();
    }
}
