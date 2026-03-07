using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Bills.Queries.Reports;

public record GetCostPerUnitReport : IQuery<CostPerUnitReport>
{
    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public Guid? AccountId { get; init; }

    public UtilityType? UtilityType { get; init; }
}

internal class GetCostPerUnitReportHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user) : IQueryHandler<GetCostPerUnitReport, CostPerUnitReport>
{
    public async ValueTask<CostPerUnitReport> Handle(GetCostPerUnitReport query, CancellationToken cancellationToken)
    {
        var userId = user.Id;
        var billsQuery = accounts.Where(a => a.Owners.Any(ah => ah.UserId == userId))
            .SelectMany(a => a.Bills)
            .Include(b => b.Account)
            .Include(b => b.Periods)
            .ThenInclude(p => p.Usage)
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

        var dataPoints = bills
            .SelectMany(b => b.Periods.Select(p => new
            {
                Date = DateOnly.FromDateTime(p.PeriodEnd),
                AccountName = b.Account?.Name ?? String.Empty,
                PricePerUnit = p.Usage?.PricePerUnit ?? 0,
                TotalUsage = p.Usage?.TotalUsage ?? 0
            }))
            .GroupBy(x => new { x.Date, x.AccountName })
            .Select(g => new CostDataPoint
            {
                Date = g.Key.Date,
                AccountName = g.Key.AccountName,
                AveragePricePerUnit = g.Average(x => x.PricePerUnit),
                TotalUsage = g.Sum(x => x.TotalUsage)
            })
            .OrderBy(x => x.Date)
            .ThenBy(x => x.AccountName)
            .ToList();

        return new CostPerUnitReport
        {
            Start = query.Start,
            End = query.End,
            DataPoints = dataPoints
        };
    }
}
