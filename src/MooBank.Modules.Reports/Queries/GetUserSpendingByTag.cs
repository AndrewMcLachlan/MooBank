using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetUserSpendingByTag : UserReportQuery, IQuery<UserSpendingByTagReport>
{
    public int? ParentTagId { get; init; }
}

internal class GetUserSpendingByTagHandler(IQueryable<LogicalAccount> accounts, IQueryable<Domain.Entities.Transactions.Transaction> transactions, User user) : IQueryHandler<GetUserSpendingByTag, UserSpendingByTagReport>
{
    public async ValueTask<UserSpendingByTagReport> Handle(GetUserSpendingByTag request, CancellationToken cancellationToken)
    {
        var (start, end) = request.ResolveRange();
        var startDateTime = start.ToStartOfDay();
        var endDateTime = end.ToEndOfDay();

        var accountIds = await accounts.AccessibleTo(user).Transactional().Select(a => a.Id).ToListAsync(cancellationToken);

        if (accountIds.Count == 0)
        {
            return new()
            {
                Start = start,
                End = end,
                Tags = [],
                IncludedAccountIds = [],
            };
        }

        var scoped = transactions
            .Specify(new IncludeTagsSpecification())
            .Where(t => accountIds.Contains(t.AccountId) && !t.ExcludeFromReporting && t.TransactionTime >= startDateTime && t.TransactionTime <= endDateTime);

        var loaded = await scoped.ToListAsync(cancellationToken);

        var perTag = loaded
            .SelectMany(t => t.Tags.Select(tag => (tag.Id, tag.Name, NetAmount: t.NetAmount)))
            .GroupBy(x => new { x.Id, x.Name })
            .Select(g => new TagValue
            {
                TagId = g.Key.Id,
                TagName = g.Key.Name,
                GrossAmount = Math.Abs(g.Sum(x => x.NetAmount)),
            })
            .ToList();

        if (request.ParentTagId == null)
        {
            var untaggedAmount = loaded.Where(t => !t.Splits.SelectMany(s => s.Tags).Any()).Sum(t => t.Amount);

            if (untaggedAmount != 0)
            {
                perTag.Add(new TagValue
                {
                    TagName = "Untagged",
                    GrossAmount = Math.Abs(untaggedAmount),
                });
            }
        }

        return new()
        {
            Start = start,
            End = end,
            Tags = perTag.OrderByDescending(t => t.GrossAmount).ToList(),
            IncludedAccountIds = accountIds,
        };
    }
}
