using Asm.MooBank.Models;

namespace Asm.MooBank.Audit;

public interface IAuditLogger
{
    void LoginSuccess(Guid userId, string email);
    void UserProvisioned(Guid userId, string email, Guid familyId);
    void AuthenticationFailed(Exception exception);

    void AuthorizationDenied(User user, string resource, Guid? resourceId, string policy);

    void HttpMutation(User user, string method, string path, string? ipAddress, int statusCode);

    void ImportStarted(User user, Guid instrumentId, Guid accountId);
    void ImportCompleted(User user, Guid instrumentId, Guid accountId, int transactionCount);
    void ImportFailed(User user, Guid instrumentId, Guid accountId, Exception exception);

    void DataChanged(User user, string action, string entityType, object? entityId);
}
