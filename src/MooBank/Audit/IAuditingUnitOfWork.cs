using Asm.Domain;
using Asm.MooBank.Models;

namespace Asm.MooBank.Audit;

public interface IAuditingUnitOfWork
{
    Task SaveChangesAsync(string action, string entityType, object? entityId = null, CancellationToken cancellationToken = default);
}

internal class AuditingUnitOfWork(IUnitOfWork unitOfWork, IAuditLogger audit, User user) : IAuditingUnitOfWork
{
    public async Task SaveChangesAsync(string action, string entityType, object? entityId = null, CancellationToken cancellationToken = default)
    {
        await unitOfWork.SaveChangesAsync(cancellationToken);
        audit.DataChanged(user, action, entityType, entityId);
    }
}
