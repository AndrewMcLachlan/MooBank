using Asm.MooBank.Commands;
using Asm.MooBank.Models;
using ITagRepository = Asm.MooBank.Domain.Entities.Tag.ITagRepository;

namespace Asm.MooBank.Modules.Tags.Commands;

public sealed record Create(Tag Tag) : ICommand<Tag>;

internal sealed class CreateHandler(IUnitOfWork unitOfWork, ITagRepository transactionTagRepository, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Tag>
{
    public async ValueTask<Tag> Handle(Create request, CancellationToken cancellationToken)
    {
        Domain.Entities.Tag.Tag tag = request.Tag.ToEntity();
        tag.FamilyId = AccountHolder.FamilyId;

        transactionTagRepository.Add(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToModel();
    }
}
