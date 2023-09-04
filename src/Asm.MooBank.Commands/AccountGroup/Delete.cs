using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.AccountGroup;

namespace Asm.MooBank.Commands.AccountGroup;

public record Delete(Guid Id) : ICommand<bool>;

internal class DeleteHandler : CommandHandlerBase, ICommandHandler<Delete, bool>
{
    private readonly IAccountGroupRepository _accountGroupRepository;

    public DeleteHandler(IAccountGroupRepository accountGroupRepository, IUnitOfWork unitOfWork, Models.AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _accountGroupRepository = accountGroupRepository;
    }

    public async Task<bool> Handle(Delete request, CancellationToken cancellationToken)
    {
        var group = await _accountGroupRepository.Get(request.Id, cancellationToken);

        Security.AssertAccountGroupPermission(group);

        _accountGroupRepository.Delete(group);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
