using Asm.MooBank.Models;
using Microsoft.Extensions.Logging;

namespace Asm.MooBank.Audit;

internal class AuditLogger(ILogger<AuditLogger> logger) : IAuditLogger
{
    public void LoginSuccess(Guid userId, string email)
    {
        using var scope = AuditScope("Authentication");
        logger.LogInformation("Successful login: {UserId} ({Email})", userId, email);
    }

    public void UserProvisioned(Guid userId, string email, Guid familyId)
    {
        using var scope = AuditScope("Authentication");
        logger.LogInformation("New user provisioned: {UserId} ({Email}), family {FamilyId}", userId, email, familyId);
    }

    public void AuthenticationFailed(Exception exception)
    {
        using var scope = AuditScope("Authentication");
        logger.LogError(exception, "Authentication failed");
    }

    public void AuthorizationDenied(User user, string resource, Guid? resourceId, string policy)
    {
        using var scope = AuditScope("Authorization");
        logger.LogWarning("Authorization denied for {UserId} on {Resource} {ResourceId} (policy: {Policy})", user.Id, resource, resourceId, policy);
    }

    public void HttpMutation(User user, string method, string path, string? ipAddress, int statusCode)
    {
        using var scope = AuditScope("HttpMutation");
        logger.LogInformation("HTTP {Method} {Path} by {UserId} from {IpAddress} -> {StatusCode}", method, path, user.Id, ipAddress, statusCode);
    }

    public void ImportStarted(User user, Guid instrumentId, Guid accountId)
    {
        using var scope = AuditScope("Import");
        logger.LogInformation("CSV import started by {UserId} for instrument {InstrumentId}, account {AccountId}", user.Id, instrumentId, accountId);
    }

    public void ImportCompleted(User user, Guid instrumentId, Guid accountId, int transactionCount)
    {
        using var scope = AuditScope("Import");
        logger.LogInformation("CSV import completed by {UserId} for instrument {InstrumentId}, account {AccountId}: {TransactionCount} transactions", user.Id, instrumentId, accountId, transactionCount);
    }

    public void ImportFailed(User user, Guid instrumentId, Guid accountId, Exception exception)
    {
        using var scope = AuditScope("Import");
        logger.LogError(exception, "CSV import failed for {UserId}, instrument {InstrumentId}, account {AccountId}", user.Id, instrumentId, accountId);
    }

    public void DataChanged(User user, string action, string entityType, object? entityId)
    {
        using var scope = AuditScope("DataChange");
        logger.LogInformation("{Action} {EntityType} {EntityId} by {UserId}", action, entityType, entityId, user.Id);
    }

    private IDisposable? AuditScope(string category) =>
        logger.BeginScope(new Dictionary<string, object>
        {
            ["AuditEvent"] = true,
            ["AuditCategory"] = category,
        });
}
