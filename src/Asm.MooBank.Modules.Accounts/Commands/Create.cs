using System.ComponentModel;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Accounts.Models.Account;
using Asm.MooBank.Services;
using ILogicalAccountRepository = Asm.MooBank.Domain.Entities.Account.ILogicalAccountRepository;

namespace Asm.MooBank.Modules.Accounts.Commands;

[DisplayName("CreateAccount")]
public record Create() : ICommand<LogicalAccount>
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public required Models.Account.InstitutionAccount InstitutionAccount { get; init; }

    public required string Currency { get; init; }

    public required decimal Balance { get; init; }

    public DateTime? OpeningDate { get; init; }

    public Guid? GroupId { get; init; }

    public AccountType AccountType { get; init; }

    public Controller Controller { get; init; }

    public bool IncludeInBudget { get; init; }

    public bool ShareWithFamily { get; init; }
}

internal class CreateHandler(ILogicalAccountRepository institutionAccountRepository, IUnitOfWork unitOfWork, User user, ICurrencyConverter currencyConverter, ISecurity security) : ICommandHandler<Create, LogicalAccount>
{
    private readonly ILogicalAccountRepository _accountRepository = institutionAccountRepository;

    public async ValueTask<LogicalAccount> Handle(Create command, CancellationToken cancellationToken)
    {
        if (command.GroupId != null)
        {
            security.AssertGroupPermission(command.GroupId.Value);
        }

        Domain.Entities.Account.LogicalAccount entity = new()
        {
            Name = command.Name,
            Description = command.Description,
            Currency = command.Currency,
            AccountType = command.AccountType,
            Controller = command.Controller,
            IncludeInBudget = command.IncludeInBudget,
            ShareWithFamily = command.ShareWithFamily,
        };

        entity.AddInstitutionAccount(command.InstitutionAccount.ToEntity());
        entity.SetAccountHolder(user.Id);
        entity.SetGroup(command.GroupId, user.Id);

        entity = _accountRepository.Add(entity, command.Balance, command.OpeningDate ?? DateTime.Now);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
