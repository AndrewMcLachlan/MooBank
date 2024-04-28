using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Tags.Commands;

internal sealed record RemoveSubTag(int Id, int SubTagId) : ICommand;

internal class RemoveSubTagHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork, ISecurity security) :  ICommandHandler<RemoveSubTag>
{
    private readonly ITagRepository _tagRepository = tagRepository;

    public async ValueTask Handle(RemoveSubTag request, CancellationToken cancellationToken)
    {
        var tag = await GetEntity(request.Id, true, cancellationToken);

        await security.AssertFamilyPermission(tag.FamilyId);

        var subTag = await GetEntity(request.SubTagId, false, cancellationToken);

        if (!tag.Tags.Any(t => t == subTag)) throw new NotFoundException($"Tag with ID {request.SubTagId} has not been removed");

        tag.Tags.Remove(subTag);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private Task<Domain.Entities.Tag.Tag> GetEntity(int id, bool includeSubTags = false, CancellationToken cancellationToken = default) => _tagRepository.Get(id, includeSubTags, cancellationToken);
}
