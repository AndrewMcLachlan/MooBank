using Asm.MooBank.Domain.Entities.Account;
using TransactionExtra = Asm.MooBank.Domain.Entities.Ing.TransactionExtra;
using Asm.MooBank.Models.Ing;
using Asm.MooBank.Models.Queries.Transactions.Ing;
using Microsoft.EntityFrameworkCore;

namespace Asm.MooBank.Services.Queries.Transactions.Ing
{
    internal class GetIngTransactionExtraDetailsHandler : IQueryHandler<GetIngTransactionExtraDetails, Models.PagedResult<Models.Transaction>>
    {
        private readonly IQueryable<TransactionExtra> _transactionExtras;
        private readonly IQueryable<Account> _accounts;

        public GetIngTransactionExtraDetailsHandler(IQueryable<TransactionExtra> transactionExtras, IQueryable<Account> accounts)
        {
            _transactionExtras = transactionExtras;
            _accounts = accounts;
        }

        public async Task<Models.PagedResult<Models.Transaction>> Handle(GetIngTransactionExtraDetails request, CancellationToken cancellationToken)
        {
            var ids = request.Transactions.Results.Select(t => t.Id);
            var extras = await _transactionExtras.Where<TransactionExtra>(te => ids.Contains(te.TransactionId)).ToListAsync(cancellationToken);

            var pagedResult = request.Transactions;

            var account = await _accounts.Include(a => a.AccountAccountHolders).ThenInclude(a => a.AccountHolder).ThenInclude(a => a.Cards).SingleAsync(a => a.AccountId == request.AccountId, cancellationToken);
            var cardNames = account.AccountAccountHolders.SelectMany(a => a.AccountHolder.Cards).ToDictionary(ac => ac.Last4Digits, ac => ac.AccountHolder.FirstName);

            foreach (var transaction in pagedResult.Results)
            {
                transaction.ExtraInfo = extras.Where<TransactionExtra>(e => e.TransactionId == transaction.Id).Select(te => te.ToModel(cardNames)).SingleOrDefault();
            }

            return pagedResult;
        }


    }
}
