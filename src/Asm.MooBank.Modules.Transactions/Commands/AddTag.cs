using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Transactions.Commands;

internal record AddTag(Guid AccountId, Guid Id, int TagId) : ICommand<Models.Transaction>;

internal class AddTagHandler(ITransactionRepository transactionRepository, ITagRepository tagRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<AddTag, Models.Transaction>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ITagRepository _tagRepository = tagRepository;

    public async ValueTask<Models.Transaction> Handle(AddTag request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var entity = await _transactionRepository.Get(request.Id, cancellationToken);

        if (entity.Tags.Any(t => t.Id == request.TagId)) throw new ExistsException("Cannot add tag, it already exists");

        var tag = await _tagRepository.Get(request.TagId, cancellationToken);

        entity.AddOrUpdateSplit(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
