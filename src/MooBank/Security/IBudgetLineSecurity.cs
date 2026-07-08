namespace Asm.MooBank.Security;

/// <summary>
/// Budget line permission checks consumed by authorisation handlers.
/// </summary>
/// <remarks>
/// Kept separate from <see cref="ISecurity"/> so implementations remain free of any
/// dependency on the authorisation system, which constructs all handlers when resolved.
/// </remarks>
public interface IBudgetLineSecurity
{
    Task<bool> HasBudgetLinePermission(Guid id, CancellationToken cancellationToken = default);
}
