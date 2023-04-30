using Asm.Domain;
using Asm.MooBank.Domain.Entities.Account;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Commands.Transaction;

namespace Asm.MooBank.Services.Commands.Transaction;

internal class SetTransactionNotesHandler : ICommandHandler<SetTransactionNotes, Models.Transaction>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ISecurityRepository _security;
    private readonly IUnitOfWork _unitOfWork;

    public SetTransactionNotesHandler(ITransactionRepository transactionRepository, ISecurityRepository securityRepository, IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _security = securityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Models.Transaction> Handle(SetTransactionNotes request, CancellationToken cancellationToken)
    {
        var entity = await _transactionRepository.Get(request.Id, cancellationToken);

        _security.AssertAccountPermission(entity.AccountId);

        entity.Notes = request.Notes;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (Models.Transaction)entity;
    }
}
