using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Modules.Bills.Queries.Reports;

public record GetServiceChargeReport : IQuery<ServiceChargeReport>
{
    public required DateOnly Start { get; init; }

    public required DateOnly End { get; init; }

    public Guid? AccountId { get; init; }

    public UtilityType? UtilityType { get; init; }
}

internal class GetServiceChargeReportHandler(IQueryable<Domain.Entities.Utility.Account> accounts, User user) : IQueryHandler<GetServiceChargeReport, ServiceChargeReport>
{
    public async ValueTask<ServiceChargeReport> Handle(GetServiceChargeReport query, CancellationToken cancellationToken)
    {
        var userId = user.Id;
        var billsQuery = accounts.Where(a => a.Owners.Any(ah => ah.UserId == userId))
            .SelectMany(a => a.Bills)
            .Include(b => b.Account)
            .Include(b => b.Periods)
            .ThenInclude(p => p.ServiceCharge)
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
                ChargePerDay = p.ServiceCharge?.ChargePerDay ?? 0
            }))
            .GroupBy(x => new { x.Date, x.AccountName })
            .Select(g => new ServiceChargeDataPoint
            {
                Date = g.Key.Date,
                AccountName = g.Key.AccountName,
                AverageChargePerDay = g.Average(x => x.ChargePerDay)
            })
            .OrderBy(x => x.Date)
            .ThenBy(x => x.AccountName)
            .ToList();

        return new ServiceChargeReport
        {
            Start = query.Start,
            End = query.End,
            DataPoints = dataPoints
        };
    }
}
