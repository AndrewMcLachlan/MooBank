using Asm.Domain;
using Asm.MooBank.Domain.Entities.Budget;
using Asm.MooBank.Models.Commands.Budget;

namespace Asm.MooBank.Services.Commands.Budget
{
    internal class CreateHandler : CommandHandlerBase, ICommandHandler<Create, Models.BudgetLine>
    {
        private readonly IBudgetRepository _budgetRepository;

        public CreateHandler(IUnitOfWork unitOfWork, IBudgetRepository budgetRepository, ISecurity security) : base(unitOfWork, security)
        {
            _budgetRepository = budgetRepository;
        }

        public async Task<Models.BudgetLine> Handle(Create request, CancellationToken cancellationToken)
        {
            Security.AssertAccountPermission(request.AccountId);

            var entity = _budgetRepository.Add(request.BudgetLine);

            await UnitOfWork.SaveChangesAsync(cancellationToken);

            return entity;
        }
    }
}
