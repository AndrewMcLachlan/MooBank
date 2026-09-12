using Asm.MooBank.Domain.Entities.Group;

namespace Asm.MooBank.Security;

/// <summary>
/// A friendly wrapper over requirement-based authorisation. Implementations evaluate
/// requirements via <c>IAuthorizationService</c>, audit denials and throw; they contain
/// no data access of their own.
/// </summary>
public interface ISecurity
{
    Task AssertGroupPermission(Guid groupId);
    Task AssertGroupPermission(Group group);

    Task AssertFamilyPermission(Guid familyId);

    Task AssertInstrumentViewer(Guid instrumentId);

    /// <summary>
    /// Asserts that every tag belongs to the current user's family. For tag ids that arrive in a
    /// request body, where no route parameter exists for the endpoint policy to check.
    /// </summary>
    Task AssertTagPermission(IReadOnlyCollection<int> tagIds);
}
