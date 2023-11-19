using ITagRepository = Asm.MooBank.Domain.Entities.Tag.ITagRepository;
using Asm.MooBank.Models;
using Asm.MooBank.Commands;

namespace Asm.MooBank.Modules.Tag.Commands;

public sealed record Create(MooBank.Models.Tag Tag) : ICommand<MooBank.Models.Tag>;

internal sealed class CreateHandler(IUnitOfWork unitOfWork, ITagRepository transactionTagRepository, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, MooBank.Models.Tag>
{
    public async ValueTask<MooBank.Models.Tag> Handle(Create request, CancellationToken cancellationToken)
    {
        Domain.Entities.Tag.Tag tag = request.Tag.ToEntity();
        tag.FamilyId = AccountHolder.FamilyId;

        transactionTagRepository.Add(tag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToModel();
    }
}
