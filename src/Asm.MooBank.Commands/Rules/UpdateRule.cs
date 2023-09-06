using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Commands.Rules;

public record UpdateRule(Guid AccountId, int RuleId, Models.Rule Rule) : ICommand<Models.Rule>;

internal class UpdateRuleHandler : ICommandHandler<UpdateRule, Models.Rule>
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

    public async Task<Models.Rule> Handle(UpdateRule request, CancellationToken cancellationToken)
    {
        var entity = await _transactionTagRuleRepository.Get(request.Rule.Id, cancellationToken);

        _security.AssertAccountPermission(entity.AccountId);

        entity.Contains = request.Rule.Contains;
        entity.Description = request.Rule.Description;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
