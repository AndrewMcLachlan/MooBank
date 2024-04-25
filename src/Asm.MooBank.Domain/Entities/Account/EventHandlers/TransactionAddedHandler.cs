using Asm.MooBank.Domain.Entities.Transactions.Events;

namespace Asm.MooBank.Domain.Entities.Account.EventHandlers;
internal class TransactionAddedHandler(IInstrumentRepository accountRepository) : IDomainEventHandler<TransactionAddedEvent>
{
    public async Task Handle(TransactionAddedEvent request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.Get(request.Transaction.AccountId, cancellationToken);

        if (account is InstitutionAccount institutionAccount && institutionAccount.AccountController == AccountController.Import)
        {
            return;
        }

        account.Balance += request.Transaction.Amount;
    }
}
