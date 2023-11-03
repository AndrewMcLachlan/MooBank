using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.TransactionTagHierarchies;
using Asm.MooBank.Models;
using ITransactionTagRepository = Asm.MooBank.Domain.Entities.Tag.ITransactionTagRepository;

namespace Asm.MooBank.Modules.Tag.Commands;

public sealed record AddSubTag(int Id, int SubId) : ICommand<Models.Tag>;

internal sealed class AddSubTagHandler : CommandHandlerBase, ICommandHandler<AddSubTag, Models.Tag>
{
    private readonly ITransactionTagRepository _transactionTagRepository;
    private readonly IQueryable<TransactionTagRelationship> _transactionTagRelationships;

    public AddSubTagHandler(ITransactionTagRepository transactionTagRepository, IQueryable<TransactionTagRelationship> transactionTagRelationships, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _transactionTagRepository = transactionTagRepository;
        _transactionTagRelationships = transactionTagRelationships;
    }

    public async ValueTask<Models.Tag> Handle(AddSubTag request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out int id, out int subId);

        if (id == subId) throw new ExistsException("Cannot add a tag to itself!");

        var tag = await GetEntity(id, true, cancellationToken);
        var subTag = await GetEntity(subId, false, cancellationToken);

        await Security.AssertFamilyPermission(tag.FamilyId);

        if (_transactionTagRelationships.Any(tr => tr.TransactionTag == subTag && tr.ParentTag == tag)) throw new ExistsException($"{subTag.Name} is already a child or grand-child of {tag.Name}");
        if (_transactionTagRelationships.Any(tr => tr.TransactionTag == tag && tr.ParentTag == subTag)) throw new ExistsException($"{subTag.Name} is parent or grand-parent of {tag.Name}. Circular relationships are not allowed!");

        tag.Tags.Add(subTag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return tag;
    }

    private Task<Domain.Entities.Tag.Tag> GetEntity(int id, bool includeSubTags = false, CancellationToken cancellationToken = default) =>
        _transactionTagRepository.Get(id, includeSubTags, cancellationToken);
}
