using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.TransactionTagHierarchies;
using Asm.MooBank.Domain.Entities.TransactionTags;
using Asm.MooBank.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services.Queries.Reports
{
    internal class GetByTagReport : IQueryHandler<Models.Queries.Reports.GetByTagReport, ByTagReport>
    {
        private readonly IQueryable<Transaction> _transactions;
        private readonly ISecurityRepository _securityRepository;

        public GetByTagReport(IQueryable<Transaction> transactions, ISecurityRepository securityRepository)
        {
            _transactions = transactions;
            _securityRepository = securityRepository;
        }

        public async Task<ByTagReport> Handle(Models.Queries.Reports.GetByTagReport request, CancellationToken cancellationToken)
        {
            _securityRepository.AssertAccountPermission(request.AccountId);

            var transactionTypeFilter = request.ReportType.ToTransactionFilter();


            var start = request.Start.ToStartOfDay();
            var end = request.End.ToEndOfDay();

            var transactions = await _transactions.Include(t => t.TransactionTags).Where(t => t.TransactionTime >= start && t.TransactionTime <= end).Where(transactionTypeFilter).ToListAsync(cancellationToken);

            var tagValuesInterim = transactions
                .GroupBy(t => t.TransactionTags)
                .SelectMany(g => g.Key.Select(t =>
                new TagValue
                {
                    TagId = t.TransactionTagId,
                    TagName = t.Name,
                    Amount = Math.Abs(g.Sum(t => t.Amount)),
                })).ToList();

            var tagValues = tagValuesInterim.GroupBy(t => new { t.TagId, t.TagName }).Select(g => new TagValue
            {
                TagId = g.Key.TagId,
                TagName = g.Key.TagName,
                Amount = g.Sum(t => t.Amount),
            }).ToList();

            if (request.TagId == null)
            {
                var tagLessAmount = await _transactions.Where(t => t.TransactionTime >= start && t.TransactionTime <= end && !t.TransactionTags.Any()).Where(transactionTypeFilter).SumAsync(t => t.Amount, cancellationToken);
                tagValues.Add(new TagValue
                {
                    TagName = "Untagged",
                    Amount = Math.Abs(tagLessAmount),
                });
            }

            return new()
            {
                AccountId = request.AccountId,
                Start = request.Start,
                End = request.End,
                Tags = tagValues.OrderByDescending(t => t.Amount),
            };
        }
    }
}
