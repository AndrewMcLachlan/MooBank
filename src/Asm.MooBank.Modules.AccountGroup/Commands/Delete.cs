using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Modules.AccountGroup.Commands;

public record Delete(Guid Id) : ICommand;

internal class DeleteHandler : CommandHandlerBase, ICommandHandler<Delete>
{
    private readonly IAccountGroupRepository _accountGroupRepository;

    public DeleteHandler(IAccountGroupRepository accountGroupRepository, IUnitOfWork unitOfWork, MooBank.Models.AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _accountGroupRepository = accountGroupRepository;
    }

    public async ValueTask Handle(Delete request, CancellationToken cancellationToken)
    {
        var group = await _accountGroupRepository.Get(request.Id, cancellationToken);

        Security.AssertAccountGroupPermission(group);

        _accountGroupRepository.Delete(group);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
