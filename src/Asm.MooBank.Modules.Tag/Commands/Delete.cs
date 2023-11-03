using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Tag;
using Asm.MooBank.Models;

namespace Asm.MooBank.Modules.Tag.Commands;

internal record Delete(int Id) : ICommand;

internal class DeleteHandler(ITransactionTagRepository tagRepository, IUnitOfWork unitOfWork, AccountHolder accountHolder, ISecurity security) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Delete>
{
    private readonly ITransactionTagRepository _tagRepository = tagRepository;

    public async ValueTask Handle(Delete command, CancellationToken cancellationToken)
    {
        var entity = await _tagRepository.Get(command.Id, false, cancellationToken);

        await Security.AssertFamilyPermission(entity.FamilyId);

        _tagRepository.Delete(entity);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
