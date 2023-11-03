using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.AccountGroup;
using Asm.MooBank.Domain.Entities.AccountHolder;

namespace Asm.MooBank.Modules.AccountGroup.Commands;


public record Create(Models.AccountGroup AccountGroup) : ICommand<Models.AccountGroup>;

internal class CreateHandler : CommandHandlerBase, ICommandHandler<Create, Models.AccountGroup>
{
    private readonly IAccountGroupRepository _accountGroupRepository;

    public CreateHandler(IAccountGroupRepository accountGroupRepository, IUnitOfWork unitOfWork, MooBank.Models.AccountHolder accountHolder, ISecurity security) : base(unitOfWork, accountHolder, security)
    {
        _accountGroupRepository = accountGroupRepository;
    }

    public async ValueTask<Models.AccountGroup> Handle(Create request, CancellationToken cancellationToken)
    {
        var entity = (Domain.Entities.AccountGroup.AccountGroup)request.AccountGroup;

        entity.OwnerId = AccountHolder.Id;

        _accountGroupRepository.Add(entity);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return (Models.AccountGroup)entity;
    }
}

