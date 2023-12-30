using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Transactions.Commands.Transactions;

internal record AddTag(Guid AccountId, Guid Id, int TagId) : ICommand<Models.Transaction>;

internal class AddTagHandler(ITransactionRepository transactionRepository, ITagRepository tagRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<AddTag, Models.Transaction>
{
    public async ValueTask<Models.Transaction> Handle(AddTag request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var entity = await transactionRepository.Get(request.Id, new IncludeSplitsSpecification(), cancellationToken);

        if (entity.Tags.Any(t => t.Id == request.TagId)) throw new ExistsException("Cannot add tag, it already exists");

        var tag = await tagRepository.Get(request.TagId, cancellationToken);

        entity.AddOrUpdateSplit(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
