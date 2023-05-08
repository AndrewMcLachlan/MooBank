using Asm.Domain;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Commands.Transaction;

namespace Asm.MooBank.Commands.Transaction;

internal class UpdateTransactionHandler : ICommandHandler<UpdateTransaction, Models.Transaction>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ISecurity _security;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTransactionHandler(ITransactionRepository transactionRepository, ISecurity securityRepository, IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _security = securityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Models.Transaction> Handle(UpdateTransaction request, CancellationToken cancellationToken)
    {
        var entity = await _transactionRepository.Get(request.Id, cancellationToken);

        _security.AssertAccountPermission(entity.AccountId);

        entity.Notes = request.Notes;
        entity.OffsetByTransactionId = request.OffsetByTransactionId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (Models.Transaction)entity;
    }
}
