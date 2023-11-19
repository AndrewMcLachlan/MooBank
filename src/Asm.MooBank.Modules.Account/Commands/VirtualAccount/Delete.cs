using Asm.MooBank.Commands;
using IAccountRepository = Asm.MooBank.Domain.Entities.Account.IAccountRepository;

namespace Asm.MooBank.Modules.Account.Commands.VirtualAccount;

public record Delete(Guid AccountId, Guid VirtualAccountId) : ICommand;

internal class DeleteHandler(IAccountRepository accountRepository, MooBank.Models.AccountHolder accountHolder, ISecurity security, IUnitOfWork unitOfWork) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Delete>
{
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async ValueTask Handle(Delete request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var account = await _accountRepository.Get(request.AccountId, cancellationToken);

        if (account is not Domain.Entities.Account.InstitutionAccount institutionAccount)
        {
            throw new InvalidOperationException("Cannot delete virtual account on non-institution account.");
        }

        var virtualAccount = institutionAccount.VirtualAccounts.SingleOrDefault(va => va.AccountId == request.VirtualAccountId) ?? throw new NotFoundException("Virtual Account not found");

        institutionAccount.VirtualAccounts.Remove(virtualAccount);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
