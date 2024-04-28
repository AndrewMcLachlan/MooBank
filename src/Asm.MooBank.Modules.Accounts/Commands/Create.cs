using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using IInstitutionAccountRepository = Asm.MooBank.Domain.Entities.Account.IInstitutionAccountRepository;

namespace Asm.MooBank.Modules.Accounts.Commands.InstitutionAccount;

public record Create() : ICommand<Models.Account.InstitutionAccount>
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public int InstitutionId { get; set; }

    public required string Currency { get; set; }

    public required decimal Balance { get; set; }

    public Guid? GroupId { get; set; }

    public AccountType AccountType { get; set; }

    public Controller Controller { get; set; }

    public int? ImporterTypeId { get; set; }

    public bool IncludeInBudget { get; init; }

    public bool ShareWithFamily { get; set; }
}

internal class CreateHandler(IInstitutionAccountRepository institutionAccountRepository, IUnitOfWork unitOfWork, User user, ICurrencyConverter currencyConverter, ISecurity security) : ICommandHandler<Create, Models.Account.InstitutionAccount>
{
    private readonly IInstitutionAccountRepository _accountRepository = institutionAccountRepository;

    public async ValueTask<Models.Account.InstitutionAccount> Handle(Create command, CancellationToken cancellationToken)
    {
        if (command.GroupId != null)
        {
            security.AssertGroupPermission(command.GroupId.Value);
        }

        Domain.Entities.Account.InstitutionAccount entity = new()
        {
            Name = command.Name,
            Description = command.Description,
            InstitutionId = command.InstitutionId,
            Currency = command.Currency,
            Balance = command.Balance,
            AccountType = command.AccountType,
            Controller = command.Controller,
            IncludeInBudget = command.IncludeInBudget,
            ShareWithFamily = command.ShareWithFamily,
        };

        entity.SetAccountHolder(user.Id);
        entity.SetGroup(command.GroupId, user.Id);

        _accountRepository.Add(entity);

        if (command.ImporterTypeId != null)
        {
            entity.ImportAccount = new Domain.Entities.Account.ImportAccount
            {
                ImporterTypeId = command.ImporterTypeId.Value,
            };
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
