using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models.Commands.Transactions;

namespace Asm.MooBank.Commands.Transaction;

public record UpdateTransaction(Guid Id, string? Notes, IEnumerable<Models.TransactionSplit> Splits, IEnumerable<CreateOffset> Offsets, bool ExcludeFromReporting = false) : ICommand<Models.Transaction>;

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

    public async Task<Models.Transaction> Handle(UpdateTransaction request, CancellationToken cancellationToken)
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
                Tags = split.Tags.Select(t => (Domain.Entities.Tag.Tag)t).ToList()
            });
        }

        foreach (var split in splitsToUpdate)
        {
            var splitEntity = entity.Splits.Single(s => s.Id == split.Id);
            splitEntity.Amount = split.Amount;
            var tags = await _tagRepository.Get(split.Tags.Select(t => t.Id), cancellationToken);
            splitEntity.UpdateTags(tags);
        }
        #endregion

        #region Offsets
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
        #endregion

        entity.Notes = request.Notes;
        entity.ExcludeFromReporting = request.ExcludeFromReporting;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (Models.Transaction)entity;
    }
}
