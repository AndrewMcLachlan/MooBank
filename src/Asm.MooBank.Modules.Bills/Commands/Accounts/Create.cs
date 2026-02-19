using System.ComponentModel;
using Asm.Domain;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Bills.Models;

namespace Asm.MooBank.Modules.Bills.Commands.Accounts;

[DisplayName("CreateBillAccount")]
public record Create : ICommand<Account>
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public required UtilityType UtilityType { get; init; }

    public required string AccountNumber { get; init; }

    public int? InstitutionId { get; init; }

    public string Currency { get; init; } = "AUD";

    public bool ShareWithFamily { get; init; } = true;
}

internal class CreateHandler(IUnitOfWork unitOfWork, Domain.Entities.Utility.IAccountRepository accountRepository, User user) : ICommandHandler<Create, Account>
{
    public async ValueTask<Account> Handle(Create command, CancellationToken cancellationToken)
    {
        Domain.Entities.Utility.Account entity = new()
        {
            Name = command.Name,
            Description = command.Description,
            Currency = command.Currency,
            Controller = Controller.Manual,
            ShareWithFamily = command.ShareWithFamily,
            UtilityType = command.UtilityType,
            AccountNumber = command.AccountNumber,
            InstitutionId = command.InstitutionId,
        };

        entity.SetAccountHolder(user.Id);

        accountRepository.Add(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
