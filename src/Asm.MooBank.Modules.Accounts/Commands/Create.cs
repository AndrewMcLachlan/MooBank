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

    public int? ImporterTypeId { get; init; }
    
    public required int InstitutionId { get; init; }

    public required string Currency { get; init; }

    public required decimal Balance { get; init; }

    public DateOnly? OpenedDate { get; init; }

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

        entity.AddInstitutionAccount(new()
        {
            Name = command.Name,
            OpenedDate = command.OpenedDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            InstitutionId = command.InstitutionId,
            ImporterTypeId = command.ImporterTypeId,
        });
        entity.SetAccountHolder(user.Id);
        entity.SetGroup(command.GroupId, user.Id);

        entity = _accountRepository.Add(entity, command.Balance, command.OpenedDate ?? DateOnly.FromDateTime(DateTime.UtcNow));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel(currencyConverter);
    }
}
