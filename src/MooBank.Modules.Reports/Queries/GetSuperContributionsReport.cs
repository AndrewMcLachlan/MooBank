using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Reports;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Reports.Models;
using TagEntity = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Reports.Queries;

public record GetSuperContributionsReport : ReportQuery, IQuery<SuperContributionsReport>;

internal class GetSuperContributionsReportHandler(
    IReportRepository repository,
    IQueryable<LogicalAccount> accounts,
    IQueryable<TagEntity> tags) : IQueryHandler<GetSuperContributionsReport, SuperContributionsReport>
{
    public async ValueTask<SuperContributionsReport> Handle(GetSuperContributionsReport request, CancellationToken cancellationToken)
    {
        var configured = await accounts
            .Where(a => a.Id == request.AccountId)
            .SelectMany(a => a.TagPurposes)
            .Where(t => t.Purpose == TagPurpose.EmployerContribution || t.Purpose == TagPurpose.PersonalContribution)
            .Select(t => new { t.Purpose, t.TagId })
            .ToListAsync(cancellationToken);

        var employerTagId = configured.FirstOrDefault(t => t.Purpose == TagPurpose.EmployerContribution)?.TagId;
        var personalTagId = configured.FirstOrDefault(t => t.Purpose == TagPurpose.PersonalContribution)?.TagId;

        var employer = await LoadSeries(repository, request, employerTagId, cancellationToken);
        var personal = await LoadSeries(repository, request, personalTagId, cancellationToken);

        var configuredTagIds = configured.Select(t => t.TagId).ToList();
        var tagNames = configuredTagIds.Count == 0
            ? new Dictionary<int, string>()
            : await tags.Where(t => configuredTagIds.Contains(t.Id))
                        .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

        return new()
        {
            AccountId = request.AccountId,
            Start = request.Start,
            End = request.End,
            EmployerTagId = employerTagId,
            EmployerTagName = employerTagId.HasValue ? tagNames.GetValueOrDefault(employerTagId.Value) : null,
            PersonalTagId = personalTagId,
            PersonalTagName = personalTagId.HasValue ? tagNames.GetValueOrDefault(personalTagId.Value) : null,
            Employer = employer,
            Personal = personal,
            EmployerTotal = employer.Sum(m => m.GrossAmount),
            PersonalTotal = personal.Sum(m => m.GrossAmount),
        };
    }

    private static async Task<List<TrendPoint>> LoadSeries(IReportRepository repository, GetSuperContributionsReport request, int? tagId, CancellationToken cancellationToken)
    {
        if (tagId is null) return [];

        var totals = await repository.GetMonthlyTotalsForTag(
            request.AccountId,
            request.Start,
            request.End,
            TransactionFilterType.Credit,
            tagId,
            cancellationToken);

        return [.. totals.ToModel()];
    }
}
