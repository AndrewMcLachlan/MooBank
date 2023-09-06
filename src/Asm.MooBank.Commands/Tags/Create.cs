using ITransactionTagRepository = Asm.MooBank.Domain.Entities.Tag.ITransactionTagRepository;
using Asm.MooBank.Models;

namespace Asm.MooBank.Commands.Tags;

public sealed record Create(Tag Tag) : ICommand<Tag>;

internal sealed class CreateHandler : CommandHandlerBase, ICommandHandler<Create, Tag>
{
    private readonly ITransactionTagRepository _transactionTagRepository;

    public CreateHandler(IUnitOfWork unitOfWork, ITransactionTagRepository transactionTagRepository, AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _transactionTagRepository = transactionTagRepository;
    }

    public async Task<Tag> Handle(Create request, CancellationToken cancellationToken)
    {
        Domain.Entities.Tag.Tag tag = request.Tag;
        tag.FamilyId = AccountHolder.FamilyId;

        _transactionTagRepository.Add(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return tag;
    }
}
