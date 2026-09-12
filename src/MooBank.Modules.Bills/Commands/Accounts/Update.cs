using System.ComponentModel;
using Asm.Domain;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Commands.Accounts;

[DisplayName("UpdateBillAccount")]
public record Update(Guid InstrumentId, UpdateBillAccount Account) : ICommand<Models.Account>;

internal class UpdateHandler(IUnitOfWork unitOfWork, Domain.Entities.Utility.IAccountRepository accountRepository) : ICommandHandler<Update, Models.Account>
{
    public async ValueTask<Models.Account> Handle(Update command, CancellationToken cancellationToken)
    {
        var entity = await accountRepository.Get(command.InstrumentId, cancellationToken);

        entity.Update(command.Account.Name, command.Account.Description, command.Account.AccountNumber, command.Account.ShareWithFamily);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
