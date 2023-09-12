using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Commands.Transactions;

namespace Asm.MooBank.Commands.Transaction;

public record UpdateTransaction(Guid Id, string? Notes, IEnumerable<CreateOffset> Offsets, bool ExcludeFromReporting = false) : ICommand<Models.Transaction>;

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


        var offsetsToRemove = entity.OffsetBy.Where(o => !request.Offsets.Any(ro => ro.TransactionOffsetId == o.OffsetTransactionId)).ToList();
        var offsetsToAdd = request.Offsets.Where(o => !entity.OffsetBy.Any(ro => ro.OffsetTransactionId == o.TransactionOffsetId)).ToList();
        var offsetsToUpdate = request.Offsets.Where(o => entity.OffsetBy.Any(ro => ro.OffsetTransactionId == o.TransactionOffsetId)).ToList();

        foreach (var offset in offsetsToRemove)
        {
            entity.RemoveOffset(offset);
        }

        foreach (var offset in offsetsToAdd)
        {
            entity.OffsetBy.Add(new TransactionOffset
            {
                Amount = offset.Amount,
                TransactionId = entity.TransactionId,
                OffsetTransactionId = offset.TransactionOffsetId,
            });
        }

        foreach (var offset in offsetsToUpdate)
        {
            var offsetEntity = entity.OffsetBy.First(o => o.OffsetTransactionId == offset.TransactionOffsetId);
            offsetEntity.Amount = offset.Amount;
        }

        entity.Notes = request.Notes;
        entity.ExcludeFromReporting = request.ExcludeFromReporting;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (Models.Transaction)entity;
    }
}
