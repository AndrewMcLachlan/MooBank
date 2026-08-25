using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Bills.Queries.Reports;

public record GetUsageReport : IQuery<UsageReport>
{
    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public Guid? AccountId { get; init; }

    public UtilityType? UtilityType { get; init; }
}

internal class GetUsageReportHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user) : IQueryHandler<GetUsageReport, UsageReport>
{
    public async ValueTask<UsageReport> Handle(GetUsageReport query, CancellationToken cancellationToken)
    {
        var userId = user.Id;
        var billsQuery = accounts.Where(a => a.Owners.Any(ah => ah.UserId == userId))
            .SelectMany(a => a.Bills)
            .Include(b => b.Account)
            .Include(b => b.Periods)
            .ThenInclude(p => p.Usages)
            .Where(b => b.IssueDate >= query.Start && b.IssueDate <= query.End)
            .AsQueryable();

        if (query.AccountId.HasValue)
        {
            billsQuery = billsQuery.Where(b => b.AccountId == query.AccountId.Value);
        }

        if (query.UtilityType.HasValue)
        {
            billsQuery = billsQuery.Where(b => b.Account.UtilityType == query.UtilityType.Value);
        }

        var bills = await billsQuery
            .OrderBy(b => b.IssueDate)
            .ToListAsync(cancellationToken);

        // A point per usage type, so solar export trends alongside consumption rather than being
        // added to it. Days come from the period, so a period contributes its length once per type.
        var dataPoints = bills
            .SelectMany(b => b.Periods.SelectMany(p => p.Usages.Select(u => new
            {
                Date = DateOnly.FromDateTime(p.PeriodEnd),
                AccountName = b.Account?.Name ?? String.Empty,
                u.UsageType,
                u.TotalUsage,
                // Billing periods are inclusive of both start and end dates.
                Days = p.DaysInclusive ?? (p.PeriodEnd - p.PeriodStart).Days + 1
            })))
            .Where(x => x.Days > 0)
            .GroupBy(x => new { x.Date, x.AccountName, x.UsageType })
            .Select(g => new UsageDataPoint
            {
                Date = g.Key.Date,
                AccountName = g.Key.AccountName,
                UsageType = g.Key.UsageType,
                UsagePerDay = g.Sum(x => x.TotalUsage) / g.Sum(x => x.Days)
            })
            .OrderBy(x => x.Date)
            .ThenBy(x => x.AccountName)
            .ThenBy(x => x.UsageType)
            .ToList();

        return new UsageReport
        {
            Start = query.Start,
            End = query.End,
            DataPoints = dataPoints
        };
    }
}
