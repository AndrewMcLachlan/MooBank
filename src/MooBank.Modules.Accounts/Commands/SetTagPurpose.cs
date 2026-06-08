using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using ILogicalAccountRepository = Asm.MooBank.Domain.Entities.Account.ILogicalAccountRepository;

namespace Asm.MooBank.Modules.Accounts.Commands;

[DisplayName("SetAccountTagPurpose")]
public record SetTagPurpose : ICommand<LogicalAccount>
{
    public required Guid InstrumentId { get; init; }

    public required TagPurpose Purpose { get; init; }

    public int? TagId { get; init; }
}

internal class SetTagPurposeHandler(IUnitOfWork unitOfWork, ILogicalAccountRepository accountRepository, ICurrencyConverter currencyConverter) : ICommandHandler<SetTagPurpose, LogicalAccount>
{
    public async ValueTask<LogicalAccount> Handle(SetTagPurpose command, CancellationToken cancellationToken)
    {
        var entity = await accountRepository.Get(command.InstrumentId, new AccountDetailsSpecification(), cancellationToken);

        entity.SetTagPurpose(command.Purpose, command.TagId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
