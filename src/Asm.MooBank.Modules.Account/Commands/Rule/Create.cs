using Asm.MooBank.Commands;
using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Modules.Account.Commands.Rule;

public record Create(Guid AccountId, string Contains, string? Description, IEnumerable<int> TagIds) : ICommand<Models.Rule>;

internal class CreateHandler(IRuleRepository ruleRepository, MooBank.Models.AccountHolder accountHolder, ISecurity security, IUnitOfWork unitOfWork) : CommandHandlerBase(unitOfWork, accountHolder, security), ICommandHandler<Create, Models.Rule>
{
    private readonly IRuleRepository _ruleRepository = ruleRepository;

    public async ValueTask<Models.Rule> Handle(Create request, CancellationToken cancellationToken)
    {
        //TODO: Deprecate Rule Repo and add to Account object

        Security.AssertAccountPermission(request.AccountId);

        var rule = new Domain.Entities.Account.Rule
        {
            AccountId = request.AccountId,
            Contains = request.Contains,
            Description = request.Description,
            Tags = request.TagIds.Select(t => new Domain.Entities.Tag.Tag { Id = t }).ToList(),
        };

        _ruleRepository.Add(rule);

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return rule;
    }
}
