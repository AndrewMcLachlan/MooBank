using ITagRepository = Asm.MooBank.Domain.Entities.Tag.ITagRepository;
using Asm.MooBank.Models;
using Asm.MooBank.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Asm.MooBank.Modules.Tag.Commands;

public sealed record CreateByName(string Name, IEnumerable<int> Tags) : ICommand<MooBank.Models.Tag>;

internal sealed class CreateByNameHandler(IUnitOfWork unitOfWork, ITagRepository transactionTagRepository, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<CreateByName, MooBank.Models.Tag>
{
    public async ValueTask<MooBank.Models.Tag> Handle(CreateByName request, CancellationToken cancellationToken)
    {
        request.Deconstruct(out string name, out IEnumerable<int> tags);

        var tagEntities = await transactionTagRepository.Get(tags, cancellationToken);

        Domain.Entities.Tag.Tag transactionTag = new()
        {
            Name = name,
            FamilyId = AccountHolder.FamilyId,
            Tags = tagEntities.ToList(),
        };
        transactionTagRepository.Add(transactionTag);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return transactionTag;
    }
}
