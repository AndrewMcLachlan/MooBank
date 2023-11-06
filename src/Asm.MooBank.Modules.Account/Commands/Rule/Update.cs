using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Modules.Account.Commands.Rule;

public record Update(Guid AccountId, int RuleId, Models.Rule Rule) : ICommand<Models.Rule>;

internal class UpdateRuleHandler : ICommandHandler<Update, Models.Rule>
{
    private readonly IRuleRepository _transactionTagRuleRepository;
    private readonly ISecurity _security;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRuleHandler(IRuleRepository transactionTagRuleRepository, ISecurity securityRepository, IUnitOfWork unitOfWork)
    {
        _transactionTagRuleRepository = transactionTagRuleRepository;
        _security = securityRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Models.Rule> Handle(Update request, CancellationToken cancellationToken)
    {
        var entity = await _transactionTagRuleRepository.Get(request.Rule.Id, cancellationToken);

        _security.AssertAccountPermission(entity.AccountId);

        entity.Contains = request.Rule.Contains;
        entity.Description = request.Rule.Description;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}
