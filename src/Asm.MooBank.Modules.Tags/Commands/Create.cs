using Asm.MooBank.Commands;
using Asm.MooBank.Models;
using ITagRepository = Asm.MooBank.Domain.Entities.Tag.ITagRepository;

namespace Asm.MooBank.Modules.Tags.Commands;

public sealed record Create(Tag Tag) : ICommand<Tag>;

internal sealed class CreateHandler(IUnitOfWork unitOfWork, ITagRepository tagRepository, User user) :  ICommandHandler<Create, Tag>
{
    public async ValueTask<Tag> Handle(Create request, CancellationToken cancellationToken)
    {
        // Security: Check not required as the tag is created against the current user's family.

        Domain.Entities.Tag.Tag tag = request.Tag.ToEntity();
        tag.FamilyId = user.FamilyId;

        tagRepository.Add(tag);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToModel();
    }
}
