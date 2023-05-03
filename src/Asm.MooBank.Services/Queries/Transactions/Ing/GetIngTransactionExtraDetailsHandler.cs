using Asm.MooBank.Domain.Entities.Ing;
using Asm.MooBank.Models.Queries.Transactions.Ing;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services.Queries.Transactions.Ing
{
    internal class GetIngTransactionExtraDetailsHandler : IQueryHandler<GetIngTransactionExtraDetails, Models.PagedResult<Models.Transaction>>
    {
        private readonly IQueryable<TransactionExtra> _transactionExtras;

        public GetIngTransactionExtraDetailsHandler(IQueryable<TransactionExtra> transactionExtras)
        {
            _transactionExtras = transactionExtras;
        }

        public async Task<Models.PagedResult<Models.Transaction>> Handle(GetIngTransactionExtraDetails request, CancellationToken cancellationToken)
        {
            var ids = request.Transactions.Results.Select(t => t.Id);
            var extras = await _transactionExtras.Where<TransactionExtra>(te => ids.Contains(te.TransactionId)).ToListAsync(cancellationToken);

            var pagedResult = request.Transactions;

            foreach (var transaction in pagedResult.Results)
            {
                transaction.ExtraInfo = extras.Where<TransactionExtra>(e => e.TransactionId == transaction.Id).Select(te => (Models.Ing.TransactionExtra)te).SingleOrDefault();
            }

            return pagedResult;
        }


    }
}
