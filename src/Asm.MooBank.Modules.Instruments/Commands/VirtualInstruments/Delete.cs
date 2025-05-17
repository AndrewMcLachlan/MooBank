using Asm.MooBank.Commands;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Instrument.IInstrumentRepository;

namespace Asm.MooBank.Modules.Instruments.Commands.VirtualInstruments;

public record Delete(Guid InstrumentId, Guid VirtualAccountId) : ICommand;

internal class DeleteHandler(IInstrumentRepository accountRepository, IUnitOfWork unitOfWork) : ICommandHandler<Delete>
{
    private readonly IInstrumentRepository _accountRepository = accountRepository;

    public async ValueTask Handle(Delete command, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.Get(command.InstrumentId, cancellationToken);

        if (account is not Domain.Entities.Account.InstitutionAccount institutionAccount)
        {
            throw new InvalidOperationException("Cannot delete virtual account on non-institution account.");
        }

        institutionAccount.RemoveVirtualInstrument(command.VirtualAccountId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
