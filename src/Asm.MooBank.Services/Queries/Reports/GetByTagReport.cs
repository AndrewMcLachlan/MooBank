using System.Linq.Expressions;
using Asm.Cqrs.Queries;
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
        private readonly IQueryable<TransactionTag> _tags;
        private readonly IQueryable<TransactionTagRelationship> _tagRelationships;
        private readonly ISecurityRepository _securityRepository;

        public GetByTagReport(IQueryable<Transaction> transactions, IQueryable<TransactionTag> tags, IQueryable<TransactionTagRelationship> tagRelationships, ISecurityRepository securityRepository)
        {
            _transactions = transactions;
            _tags = tags;
            _tagRelationships = tagRelationships;
            _securityRepository = securityRepository;
        }

        public async Task<ByTagReport> Handle(Models.Queries.Reports.GetByTagReport request, CancellationToken cancellationToken)
        {
            _securityRepository.AssertAccountPermission(request.AccountId);

            var transactionTypeFilter = request.ReportType.ToTransactionFilter();

            IEnumerable<TransactionTag> tags;
            tags = request.TagId == null ? _tags.Where(t => !t.TaggedTo.Any()) : _tags.Where(t => t.TaggedTo.Any(t2 => t2.TransactionTagId == request.TagId));
            var tagHierarchies = await _tagRelationships.Include(t => t.TransactionTag).Include(t => t.ParentTag).Where(tr => tags.Contains(tr.ParentTag)).ToListAsync(cancellationToken);
            var allTags = tagHierarchies.Select(t => t.TransactionTag).ToList();

            var start = request.Start.ToStartOfDay();
            var end = request.End.ToEndOfDay();

            var transactions = await _transactions.Include(t => t.TransactionTags).Where(t => t.TransactionTime >= start && t.TransactionTime <= end && t.TransactionTags.Any(tt => allTags.Contains(tt))).Where(transactionTypeFilter).ToListAsync(cancellationToken);

            var tagValues = transactions
                .GroupBy(t => tags.FirstOrDefault(tag => t.TransactionTags.Contains(tag)) ?? tagHierarchies.Where(tag => t.TransactionTags.Contains(tag.TransactionTag)).Select(tag => tag.ParentTag).First())
                .Select(g => new TagValue
                {
                    TagId = g.Key.TransactionTagId,
                    TagName = g.Key.Name,
                    Amount = Math.Abs(g.Sum(t => t.Amount)),
                }).ToList();

            if (request.TagId == null)
            {
                var tagLessAmount = await _transactions.Where(t => t.TransactionTime >= start && t.TransactionTime <= end && !t.TransactionTags.Any()).Where(transactionTypeFilter).SumAsync(t => t.Amount, cancellationToken);
                tagValues.Add(new TagValue {
                    TagName = "Untagged",
                    Amount = Math.Abs(tagLessAmount),
                });
            }



            /*var sum = transactions.Sum(t => t.Amount);

            foreach (var t in tagValues)
            {
                t.Percent = (t.Amount / sum) * 100.0m;
            }*/

            return new()
            {
                AccountId = request.AccountId,
                Start = request.Start,
                End = request.End,
                Tags = tagValues,
            };
        }
    }
}
