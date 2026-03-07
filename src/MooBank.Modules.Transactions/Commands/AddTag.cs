using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Domain.Entities.Transactions;
using Asm.MooBank.Domain.Entities.Transactions.Specifications;
using Asm.MooBank.Models;
using Asm.MooBank.Modules.Transactions.Models.Extensions;

namespace Asm.MooBank.Modules.Transactions.Commands;

internal record AddTag(Guid InstrumentId, Guid Id, int TagId) : ICommand<MooBank.Models.Transaction>;

internal class AddTagHandler(ITransactionRepository transactionRepository, ITagRepository tagRepository, IUnitOfWork unitOfWork) : ICommandHandler<AddTag, MooBank.Models.Transaction>
{
    public async ValueTask<MooBank.Models.Transaction> Handle(AddTag request, CancellationToken cancellationToken)
    {
        var entity = await transactionRepository.Get(request.Id, new IncludeSplitsSpecification(), cancellationToken);

        if (entity.Tags.Any(t => t.Id == request.TagId)) throw new ExistsException("Cannot add tag, it already exists");

        var tag = await tagRepository.Get(request.TagId, cancellationToken);

        entity.AddOrUpdateSplit(tag);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
