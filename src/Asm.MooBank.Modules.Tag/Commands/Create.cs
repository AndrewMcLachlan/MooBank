using ITransactionTagRepository = Asm.MooBank.Domain.Entities.Tag.ITransactionTagRepository;
using Asm.MooBank.Models;
using Asm.MooBank.Commands;

namespace Asm.MooBank.Modules.Tag.Commands;

public sealed record Create(Models.Tag Tag) : ICommand<Models.Tag>;

internal sealed class CreateHandler : CommandHandlerBase, ICommandHandler<Create, Models.Tag>
{
    private readonly ITransactionTagRepository _transactionTagRepository;

    public CreateHandler(IUnitOfWork unitOfWork, ITransactionTagRepository transactionTagRepository, AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _transactionTagRepository = transactionTagRepository;
    }

    public async ValueTask<Models.Tag> Handle(Create request, CancellationToken cancellationToken)
    {
        Domain.Entities.Tag.Tag tag = request.Tag.ToEntity();
        tag.FamilyId = AccountHolder.FamilyId;

        _transactionTagRepository.Add(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return tag;
    }
}
