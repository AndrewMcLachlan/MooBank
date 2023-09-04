using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Commands.TransactionTags;

public sealed record Update(int TagId, string Name, bool ExcludeFromReporting, bool ApplySmoothing) : ICommand<Models.Tag>;

internal sealed class UpdateHandler : CommandHandlerBase, ICommandHandler<Update, Models.Tag>
{
    private readonly ITransactionTagRepository _transactionTagRepository;

    public UpdateHandler(ITransactionTagRepository transactionTagRepository, IUnitOfWork unitOfWork, Models.AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _transactionTagRepository = transactionTagRepository;
    }

    public async Task<Models.Tag> Handle(Update request, CancellationToken cancellationToken)
    {
        var tag = await _transactionTagRepository.Get(request.TagId, cancellationToken);

        await Security.AssertFamilyPermission(tag.FamilyId);

        tag.Name = request.Name;
        tag.Settings.ExcludeFromReporting = request.ExcludeFromReporting;
        tag.Settings.ApplySmoothing = request.ApplySmoothing;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return tag;
    }
}
