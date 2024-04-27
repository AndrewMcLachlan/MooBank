using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models.Extensions;

namespace Asm.MooBank.Modules.Transactions.Commands;

internal record RemoveTag(Guid AccountId, Guid Id, int TagId) : ICommand<Models.Transaction>;

internal class RemoveTagHandler(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, ISecurity security) :  ICommandHandler<RemoveTag, Models.Transaction>
{
    public async ValueTask<Models.Transaction> Handle(RemoveTag request, CancellationToken cancellationToken)
    {
        security.AssertAccountPermission(request.AccountId);

        var entity = await transactionRepository.Get(request.Id, new IncludeSplitsSpecification(), cancellationToken);

        var tag = entity.Tags.SingleOrDefault(t => t.Id == request.TagId) ?? throw new NotFoundException("Tag not found");

        entity.UpdateOrRemoveSplit(tag);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
