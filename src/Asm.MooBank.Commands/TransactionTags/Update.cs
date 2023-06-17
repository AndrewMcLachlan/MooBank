using Asm.MooBank.Domain.Entities.TransactionTags;

namespace Asm.MooBank.Commands.TransactionTags;

public sealed record Update(int TagId, string Name, bool ExcludeFromReporting, bool ApplySmoothing) : ICommand<Models.TransactionTag>;

internal sealed class UpdateHandler : DataCommandHandler, ICommandHandler<Update, Models.TransactionTag>
{
    private readonly ITransactionTagRepository _transactionTagRepository;

    public UpdateHandler(ITransactionTagRepository transactionTagRepository, IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _transactionTagRepository = transactionTagRepository;
    }

    public async Task<Models.TransactionTag> Handle(Update request, CancellationToken cancellationToken)
    {
        var tag = await _transactionTagRepository.Get(request.TagId, cancellationToken);
        tag.Name = request.Name;
        tag.Settings.ExcludeFromReporting = request.ExcludeFromReporting;
        tag.Settings.ApplySmoothing = request.ApplySmoothing;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return tag;
    }
}
