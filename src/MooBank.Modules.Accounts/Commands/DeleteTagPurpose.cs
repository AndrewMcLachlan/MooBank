using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using ILogicalAccountRepository = Asm.MooBank.Domain.Entities.Account.ILogicalAccountRepository;

namespace Asm.MooBank.Modules.Accounts.Commands;

[DisplayName("DeleteAccountTagPurpose")]
public record DeleteTagPurpose : ICommand<LogicalAccount>
{
    public required Guid InstrumentId { get; init; }

    public required TagPurpose Purpose { get; init; }
}

internal class DeleteTagPurposeHandler(IUnitOfWork unitOfWork, ILogicalAccountRepository accountRepository, ICurrencyConverter currencyConverter) : ICommandHandler<DeleteTagPurpose, LogicalAccount>
{
    public async ValueTask<LogicalAccount> Handle(DeleteTagPurpose command, CancellationToken cancellationToken)
    {
        var entity = await accountRepository.Get(command.InstrumentId, new AccountDetailsSpecification(), cancellationToken);

        entity.SetTagPurpose(command.Purpose, null);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
