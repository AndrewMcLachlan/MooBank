using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.TagRelationships;
using Asm.MooBank.Models;
using ITagRepository = Asm.MooBank.Domain.Entities.Tag.ITagRepository;

namespace Asm.MooBank.Modules.Tag.Commands;

public sealed record AddSubTag(int Id, int SubTagId) : ICommand<MooBank.Models.Tag>;

internal sealed class AddSubTagHandler(ITagRepository tagRepository, IEnumerable<TagRelationship> transactionTagRelationships, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<AddSubTag, MooBank.Models.Tag>
{
    public async ValueTask<MooBank.Models.Tag> Handle(AddSubTag request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out int id, out int subId);

        if (id == subId) throw new ExistsException("Cannot add a tag to itself!");

        var tag = await GetEntity(id, true, cancellationToken);
        var subTag = await GetEntity(subId, false, cancellationToken);

        await Security.AssertFamilyPermission(tag.FamilyId);

        if (transactionTagRelationships.Any(tr => tr.TransactionTag == subTag && tr.ParentTag == tag)) throw new ExistsException($"{subTag.Name} is already a child or grand-child of {tag.Name}");
        if (transactionTagRelationships.Any(tr => tr.TransactionTag == tag && tr.ParentTag == subTag)) throw new ExistsException($"{subTag.Name} is parent or grand-parent of {tag.Name}. Circular relationships are not allowed!");

        tag.Tags.Add(subTag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToModel();
    }

    private Task<Domain.Entities.Tag.Tag> GetEntity(int id, bool includeSubTags = false, CancellationToken cancellationToken = default) =>
        tagRepository.Get(id, includeSubTags, cancellationToken);
}
