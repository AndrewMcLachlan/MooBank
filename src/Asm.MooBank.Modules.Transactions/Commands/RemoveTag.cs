using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Transactions.Commands;

internal record RemoveTag(Guid AccountId, Guid Id, int TagId) : ICommand<Models.Transaction>;

internal class RemoveTagHandler(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<RemoveTag, Models.Transaction>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async ValueTask<Models.Transaction> Handle(RemoveTag request, CancellationToken cancellationToken)
    {
        Security.AssertAccountPermission(request.AccountId);

        var entity = await _transactionRepository.Get(request.Id, cancellationToken);

        var tag = entity.Tags.SingleOrDefault(t => t.Id == request.TagId) ?? throw new NotFoundException("Tag not found");

        entity.UpdateOrRemoveSplit(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
