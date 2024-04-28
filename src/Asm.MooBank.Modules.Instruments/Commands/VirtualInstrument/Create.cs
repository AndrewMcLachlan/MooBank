using Asm.MooBank.Domain.Entities.Instrument;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Instruments.Models.Account;
using Asm.MooBank.Services;

namespace Asm.MooBank.Modules.Instruments.Commands.VirtualAccount;

public record Create(Guid InstrumentId, VirtualInstrument VirtualInstrument) : ICommand<VirtualInstrument>;

internal class CreateHandler(IInstrumentRepository instrumentRepository, Domain.Entities.Transactions.ITransactionRepository transactionRepository, ISecurity security, IUnitOfWork unitOfWork, ICurrencyConverter currencyConverter) : ICommandHandler<Create, VirtualInstrument>
{
    public async ValueTask<VirtualInstrument> Handle(Create request, CancellationToken cancellationToken)
    {
        security.AssertInstrumentPermission(request.InstrumentId);
        var instrument = await instrumentRepository.Get(request.InstrumentId, cancellationToken);

        var entity = request.VirtualInstrument.ToEntity(request.InstrumentId);

        // TODO: Should use domain events here.
        if (entity.Balance != 0)
        {
            transactionRepository.Add(new Domain.Entities.Transactions.Transaction
            {
                Account = entity,
                Amount = entity.Balance,
                Description = "Initial balance",
                Source = "Web",
                TransactionTime = DateTime.Now,
                TransactionType = entity.Balance > 0 ? TransactionType.BalanceAdjustmentCredit : TransactionType.BalanceAdjustmentDebit,
            });
        }

        instrument.VirtualInstruments.Add(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
