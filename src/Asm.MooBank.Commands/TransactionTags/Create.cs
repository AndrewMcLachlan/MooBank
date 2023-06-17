using ITransactionTagRepository = Asm.MooBank.Domain.Entities.TransactionTags.ITransactionTagRepository;
using Asm.MooBank.Models;

namespace Asm.MooBank.Commands.TransactionTags;

public sealed record Create(TransactionTag Tag) : ICommand<TransactionTag>;

internal sealed class CreateHandler : DataCommandHandler, ICommandHandler<Create, TransactionTag>
{
    private readonly ITransactionTagRepository _transactionTagRepository;

    public CreateHandler(IUnitOfWork unitOfWork, ITransactionTagRepository transactionTagRepository) : base(unitOfWork)
    {
        _transactionTagRepository = transactionTagRepository;
    }

    public async Task<TransactionTag> Handle(Create request, CancellationToken cancellationToken)
    {
        Domain.Entities.TransactionTags.TransactionTag transactionTag = request.Tag;
        _transactionTagRepository.Add(transactionTag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return transactionTag;
    }
}
