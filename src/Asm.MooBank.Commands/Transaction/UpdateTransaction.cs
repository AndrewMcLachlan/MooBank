using Asm.MooBank.Domain.Entities.Transactions;

namespace Asm.MooBank.Commands.Transaction;

public record UpdateTransaction(Guid Id, string? Notes, Guid? OffsetByTransactionId, bool ExcludeFromReporting = false) : ICommand<Models.Transaction>;

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
        entity.ExcludeFromReporting = request.ExcludeFromReporting;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (Models.Transaction)entity;
    }
}
