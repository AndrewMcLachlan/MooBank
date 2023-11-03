using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Commands.Transactions;

namespace Asm.MooBank.Modules.Transactions.Commands;

public record UpdateTransaction(Guid Id, string? Notes, IEnumerable<Models.TransactionSplit> Splits, bool ExcludeFromReporting = false) : ICommand<Models.Transaction>;

internal class UpdateTransactionHandler : ICommandHandler<UpdateTransaction, Models.Transaction>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionTagRepository _tagRepository;
    private readonly ISecurity _security;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTransactionHandler(ITransactionRepository transactionRepository, ITransactionTagRepository tagRepository, ISecurity securityRepository, IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _tagRepository = tagRepository;
        _security = securityRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Models.Transaction> Handle(UpdateTransaction request, CancellationToken cancellationToken)
    {
        var entity = await _transactionRepository.Get(request.Id, cancellationToken);

        _security.AssertAccountPermission(entity.AccountId);

        #region Splits
        var splitsToRemove = entity.Splits.Where(o => !request.Splits.Any(ro => ro.Id == o.Id)).ToList();
        var splitsToAdd = request.Splits.Where(o => !entity.Splits.Any(ro => ro.Id == o.Id)).ToList();
        var splitsToUpdate = request.Splits.Where(o => entity.Splits.Any(ro => ro.Id == o.Id)).ToList();

        foreach (var split in splitsToRemove)
        {
            entity.RemoveSplit(split);
        }

        foreach (var split in splitsToAdd)
        {
            entity.Splits.Add(new TransactionSplit(split.Id)
            {
                Amount = split.Amount,
                TransactionId = entity.TransactionId,
                Tags = (await _tagRepository.Get(split.Tags.Select(t => t.Id), cancellationToken)).ToList(),
            });
        }

        foreach (var split in splitsToUpdate)
        {
            var splitEntity = entity.Splits.Single(s => s.Id == split.Id);
            splitEntity.Amount = split.Amount;
            var tags = await _tagRepository.Get(split.Tags.Select(t => t.Id), cancellationToken);
            splitEntity.UpdateTags(tags);

            #region Offsets
            var offsetsToRemove = splitEntity.OffsetBy.Where(o => !split.OffsetBy.Any(ro => ro.Transaction.Id == o.OffsetTransactionId)).ToList();
            var offsetsToAdd = split.OffsetBy.Where(o => !splitEntity.OffsetBy.Any(ro => ro.OffsetTransactionId == o.Transaction.Id)).ToList();
            var offsetsToUpdate = split.OffsetBy.Where(o => splitEntity.OffsetBy.Any(ro => ro.OffsetTransactionId == o.Transaction.Id)).ToList();

            foreach (var offset in offsetsToRemove)
            {
                splitEntity.RemoveOffset(offset);
            }

            foreach (var offset in offsetsToAdd)
            {
                splitEntity.OffsetBy.Add(new TransactionOffset
                {
                    Amount = offset.Amount,
                    TransactionSplitId = splitEntity.Id,
                    OffsetTransactionId = offset.Transaction.Id,
                });
            }

            foreach (var offset in offsetsToUpdate)
            {
                var offsetEntity = splitEntity.OffsetBy.First(o => o.OffsetTransactionId == offset.Transaction.Id);
                offsetEntity.Amount = offset.Amount;
            }
            #endregion
        }
        #endregion

        entity.Notes = request.Notes;
        entity.ExcludeFromReporting = request.ExcludeFromReporting;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (Models.Transaction)entity;
    }
}
