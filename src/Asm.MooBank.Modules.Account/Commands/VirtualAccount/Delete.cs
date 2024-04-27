﻿using Asm.MooBank.Commands;
using IInstrumentRepository = Asm.MooBank.Domain.Entities.Account.IInstrumentRepository;

namespace Asm.MooBank.Modules.Accounts.Commands.VirtualAccount;

public record Delete(Guid AccountId, Guid VirtualAccountId) : ICommand;

internal class DeleteHandler(IInstrumentRepository accountRepository, ISecurity security, IUnitOfWork unitOfWork) :  ICommandHandler<Delete>
{
    private readonly IInstrumentRepository _accountRepository = accountRepository;

    public async ValueTask Handle(Delete request, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(request.AccountId);

        var account = await _accountRepository.Get(request.AccountId, cancellationToken);

        if (account is not Domain.Entities.Account.InstitutionAccount institutionAccount)
        {
            throw new InvalidOperationException("Cannot delete virtual account on non-institution account.");
        }

        var virtualAccount = institutionAccount.VirtualInstruments.SingleOrDefault(va => va.Id == request.VirtualAccountId) ?? throw new NotFoundException("Virtual Account not found");

        institutionAccount.VirtualInstruments.Remove(virtualAccount);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
