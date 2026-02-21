using System.ComponentModel;
using Asm.MooBank.Domain.Entities.Tag;

namespace Asm.MooBank.Modules.Tags.Commands;

public record Delete(int Id) : ICommand;

[DisplayName("DeleteTag")]
internal class DeleteHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork, ISecurity security) : ICommandHandler<Delete>
{
    private readonly ITagRepository _tagRepository = tagRepository;

    public async ValueTask Handle(Delete command, CancellationToken cancellationToken)
    {
        var entity = await _tagRepository.Get(command.Id, false, cancellationToken);

        await security.AssertFamilyPermission(entity.FamilyId);

        _tagRepository.Delete(entity);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
