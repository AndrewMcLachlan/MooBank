namespace Asm.MooBank.Domain.Entities.Retirement;

public interface IRetirementRepository : IDeletableRepository<RetirementPlan, Guid>, IWritableRepository<RetirementPlan, Guid>
{
}
