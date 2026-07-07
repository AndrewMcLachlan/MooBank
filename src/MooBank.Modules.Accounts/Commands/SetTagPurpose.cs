using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Account.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using ILogicalAccountRepository = Asm.MooBank.Domain.Entities.Account.ILogicalAccountRepository;
using ITagRepository = Asm.MooBank.Domain.Entities.Tag.ITagRepository;

namespace Asm.MooBank.Modules.Accounts.Commands;

[DisplayName("SetAccountTagPurpose")]
public record SetTagPurpose : ICommand<LogicalAccount>
{
    public required Guid InstrumentId { get; init; }

    public required TagPurpose Purpose { get; init; }

    public required int TagId { get; init; }
}

internal class SetTagPurposeHandler(IUnitOfWork unitOfWork, ILogicalAccountRepository accountRepository, ITagRepository tagRepository, ICurrencyConverter currencyConverter) : ICommandHandler<SetTagPurpose, LogicalAccount>
{
    public async ValueTask<LogicalAccount> Handle(SetTagPurpose command, CancellationToken cancellationToken)
    {
        var entity = await accountRepository.Get(command.InstrumentId, new AccountDetailsSpecification(), cancellationToken);

        // Resolve the tag via the family-scoped repository to ensure it belongs to the user's family.
        var tag = await tagRepository.Get(command.TagId, cancellationToken);

        entity.SetTagPurpose(command.Purpose, tag.Id);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
