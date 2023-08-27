using ITransactionTagRepository = Asm.MooBank.Domain.Entities.Tag.ITransactionTagRepository;
using Asm.MooBank.Models;

namespace Asm.MooBank.Commands.TransactionTags;

public sealed record CreateByName(string Name, IEnumerable<int> Tags) : ICommand<TransactionTag>;

internal sealed class CreateByNameHandler : DataCommandHandler, ICommandHandler<CreateByName, TransactionTag>
{
    private readonly ITransactionTagRepository _transactionTagRepository;

    public CreateByNameHandler(IUnitOfWork unitOfWork, ITransactionTagRepository transactionTagRepository) : base(unitOfWork)
    {
        _transactionTagRepository = transactionTagRepository;
    }

    public async Task<TransactionTag> Handle(CreateByName request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out string name, out IEnumerable<int> tags);

        var tagEntities = await _transactionTagRepository.Get(tags);

        Domain.Entities.Tag.Tag transactionTag = new()
        {
            Name = name,
            Tags = tagEntities.ToList(),
        };
        _transactionTagRepository.Add(transactionTag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return transactionTag;
    }
}
