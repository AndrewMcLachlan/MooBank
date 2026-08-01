namespace Asm.MooBank.Domain;

/// <summary>
/// Data queries used by authorisation requirement handlers.
/// </summary>
/// <remarks>
/// Implementations must not depend on the authorisation system: requirement handlers are
/// constructed when <c>IAuthorizationService</c> is resolved, so any such dependency is circular.
/// Authorisation decisions belong in the handlers; this reader only answers questions of fact.
/// </remarks>
public interface IAuthorisationReader
{
    Task<bool> IsGroupOwner(Guid groupId, Guid userId, CancellationToken cancellationToken = default);

    Task<Guid?> GetBudgetLineFamilyId(Guid budgetLineId, CancellationToken cancellationToken = default);

    Task<Guid?> GetTagFamilyId(int tagId, CancellationToken cancellationToken = default);

    Task<Guid?> GetForecastPlanFamilyId(Guid planId, CancellationToken cancellationToken = default);

    Task<Guid?> GetRetirementPlanFamilyId(Guid planId, CancellationToken cancellationToken = default);
}
