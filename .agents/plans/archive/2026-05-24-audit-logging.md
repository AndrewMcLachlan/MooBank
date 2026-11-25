# Audit Logging Implementation Plan

Addresses [#722 — Add comprehensive audit logging (T-R1)](https://github.com/AndrewMcLachlan/MooBank/issues/722).

---

## Context

MooBank currently has no formalised audit trail for security-sensitive operations. Authentication failures are logged via Serilog, and the import service has ad-hoc `ILogger` calls, but there is no consistent, queryable audit record of who did what and when.

The threat model identifies this as **T-R1: Insufficient Audit Logging** (Medium severity). With only a couple of users, a heavyweight database audit table with change-tracking interceptors would be over-engineered. Instead, this plan uses **structured logging to Seq** as the primary audit mechanism — Seq already provides search, dashboards, alerting, and retention policies. A database audit table can be added later if in-app audit viewing becomes a requirement.

---

## Design Decisions

### Injectable, logging-framework-agnostic

The audit logger is an `IAuditLog` interface backed by `Microsoft.Extensions.Logging.ILogger<AuditLog>`, not Serilog directly. This keeps Serilog at arm's length — the implementation can be swapped without touching callers.

### User-derived properties

Where possible, audit methods accept the `User` model and derive `UserId`, `Email`, and `FamilyId` internally. Callers don't pass emails or IDs around. The only exceptions are auth events in `OnTokenValidated` / `OnAuthenticationFailed` where a `User` model isn't yet available — these accept primitives.

### SaveChangesAsync audit overload

An extension method on `IUnitOfWork` wraps `SaveChangesAsync` to log an audit event on success. Command handlers opt in by calling the overload with audit parameters. If SaveChanges throws, no audit entry is written — the audit trail reflects only persisted changes.

### What goes to Seq (structured logging)

All audit events are emitted as structured log entries with a consistent `AuditEvent = true` scope property for filtering in Seq.

**Events to log:**

| Category | Event | Key Properties |
|----------|-------|----------------|
| Authentication | Successful login | UserId, Email |
| Authentication | Login failure | Exception details |
| Authentication | New user provisioned | UserId, Email, FamilyId |
| Authorization | Permission denied | UserId, Resource, ResourceId, Policy |
| Data mutation | Any POST/PUT/PATCH/DELETE | UserId, IP, Method, Path, StatusCode |
| Data change | SaveChanges with audit | UserId, Action, EntityType, EntityId |
| Import | CSV import started | UserId, InstrumentId, AccountId |
| Import | CSV import completed | UserId, InstrumentId, AccountId, TransactionCount |
| Import | CSV import failed | UserId, InstrumentId, AccountId, Error |

### What goes to the database

Nothing, for now. Seq provides equivalent queryability without schema/migration overhead. The structured properties on each log event map directly to table columns if a database table is needed later.

### What is NOT logged

- **Read operations (GET requests)** — the threat model focuses on repudiation of writes
- **Per-entity change tracking (old/new values)** — disproportionate complexity for a 2-user app

---

## Implementation

### 1. Create `IAuditLog` interface and implementation

**File:** `src/MooBank/Audit/IAuditLog.cs` (new)

```csharp
namespace Asm.MooBank.Audit;

public interface IAuditLog
{
    // Auth events — accept primitives because User model isn't available during token validation
    void LoginSuccess(Guid userId, string email);
    void UserProvisioned(Guid userId, string email, Guid familyId);
    void AuthenticationFailed(Exception exception);

    // Authorization — User available
    void AuthorizationDenied(User user, string resource, Guid? resourceId, string policy);

    // HTTP-level mutation
    void HttpMutation(User user, string method, string path, string? ipAddress, int statusCode);

    // Import
    void ImportStarted(User user, Guid instrumentId, Guid accountId);
    void ImportCompleted(User user, Guid instrumentId, Guid accountId, int transactionCount);
    void ImportFailed(User user, Guid instrumentId, Guid accountId, Exception exception);

    // Generic data change — used by SaveChangesAsync overload
    void DataChanged(User user, string action, string entityType, Guid? entityId);
}
```

**File:** `src/MooBank/Audit/AuditLog.cs` (new)

Implementation uses `ILogger<AuditLog>` with `BeginScope` to stamp each entry with `AuditEvent = true` and an `AuditCategory`. This works with any logging provider — Serilog, OpenTelemetry, etc.

```csharp
namespace Asm.MooBank.Audit;

internal class AuditLog(ILogger<AuditLog> logger) : IAuditLog
{
    public void LoginSuccess(Guid userId, string email)
    {
        using var scope = AuditScope("Authentication");
        logger.LogInformation("Successful login: {UserId} ({Email})", userId, email);
    }

    public void UserProvisioned(Guid userId, string email, Guid familyId)
    {
        using var scope = AuditScope("Authentication");
        logger.LogInformation("New user provisioned: {UserId} ({Email}), family {FamilyId}",
            userId, email, familyId);
    }

    public void AuthenticationFailed(Exception exception)
    {
        using var scope = AuditScope("Authentication");
        logger.LogError(exception, "Authentication failed");
    }

    public void AuthorizationDenied(User user, string resource, Guid? resourceId, string policy)
    {
        using var scope = AuditScope("Authorization");
        logger.LogWarning("Authorization denied for {UserId} on {Resource} {ResourceId} (policy: {Policy})",
            user.Id, resource, resourceId, policy);
    }

    public void HttpMutation(User user, string method, string path, string? ipAddress, int statusCode)
    {
        using var scope = AuditScope("HttpMutation");
        logger.LogInformation("HTTP {Method} {Path} by {UserId} from {IpAddress} -> {StatusCode}",
            method, path, user.Id, ipAddress, statusCode);
    }

    public void ImportStarted(User user, Guid instrumentId, Guid accountId)
    {
        using var scope = AuditScope("Import");
        logger.LogInformation("CSV import started by {UserId} for instrument {InstrumentId}, account {AccountId}",
            user.Id, instrumentId, accountId);
    }

    public void ImportCompleted(User user, Guid instrumentId, Guid accountId, int transactionCount)
    {
        using var scope = AuditScope("Import");
        logger.LogInformation("CSV import completed by {UserId} for instrument {InstrumentId}, account {AccountId}: {TransactionCount} transactions",
            user.Id, instrumentId, accountId, transactionCount);
    }

    public void ImportFailed(User user, Guid instrumentId, Guid accountId, Exception exception)
    {
        using var scope = AuditScope("Import");
        logger.LogError(exception, "CSV import failed for {UserId}, instrument {InstrumentId}, account {AccountId}",
            user.Id, instrumentId, accountId);
    }

    public void DataChanged(User user, string action, string entityType, Guid? entityId)
    {
        using var scope = AuditScope("DataChange");
        logger.LogInformation("{Action} {EntityType} {EntityId} by {UserId}",
            action, entityType, entityId, user.Id);
    }

    private IDisposable? AuditScope(string category) =>
        logger.BeginScope(new Dictionary<string, object>
        {
            ["AuditEvent"] = true,
            ["AuditCategory"] = category,
        });
}
```

### 2. Create `IUnitOfWork` audit extension

**File:** `src/MooBank/Audit/UnitOfWorkAuditExtensions.cs` (new)

Extension method that wraps `SaveChangesAsync` — logs only on success.

```csharp
namespace Asm.MooBank.Audit;

public static class UnitOfWorkAuditExtensions
{
    public static async Task SaveChangesAsync(
        this IUnitOfWork unitOfWork,
        IAuditLog audit,
        User user,
        string action,
        string entityType,
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        await unitOfWork.SaveChangesAsync(cancellationToken);
        audit.DataChanged(user, action, entityType, entityId);
    }
}
```

Command handlers opt in:
```csharp
// Before (no audit):
await unitOfWork.SaveChangesAsync(cancellationToken);

// After (with audit):
await unitOfWork.SaveChangesAsync(audit, user, "Created", "Account", entity.Id, cancellationToken);
```

Not every `SaveChangesAsync` call needs auditing (e.g. reference data updates, stock price fetches). Only command handlers for user-facing financial data mutations use the overload.

### 3. Register in DI

**File:** `src/MooBank/ServiceCollectionExtensions.cs` (modify)

Add `services.AddScoped<IAuditLog, AuditLog>()` to the `AddServices` method. Scoped lifetime matches the request/user lifecycle.

### 4. Create audit middleware

**File:** `src/MooBank.Api/Middleware/AuditMiddleware.cs` (new)

Logs all authenticated mutating HTTP requests (POST, PUT, PATCH, DELETE). Resolves `IAuditLog` and `User` from the request scope after the pipeline completes.

```csharp
public class AuditMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> AuditedMethods = ["POST", "PUT", "PATCH", "DELETE"];

    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (!AuditedMethods.Contains(context.Request.Method)) return;
        if (context.User.Identity?.IsAuthenticated != true) return;

        var audit = context.RequestServices.GetRequiredService<IAuditLog>();
        var user = context.RequestServices.GetRequiredService<User>();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        audit.HttpMutation(user, context.Request.Method, context.Request.Path.Value!,
            ipAddress, context.Response.StatusCode);
    }
}
```

**File:** `src/MooBank.Api/Program.cs` (modify)

Add `app.UseMiddleware<AuditMiddleware>()` in `AddApp`, after `app.UseAuthorization()`.

### 5. Add auth event audit logging

**File:** `src/MooBank.Api/IServiceCollectionExtensions.cs` (modify)

Auth events happen during token validation, before a `User` model is available — these are the cases where `IAuditLog` accepts primitives.

- `OnTokenValidated`: After the claims cache block resolves, resolve `IAuditLog` from `context.HttpContext.RequestServices` and call `audit.LoginSuccess(userId, email)`. Inside the new-user creation block, call `audit.UserProvisioned(userId, email, familyId)`.
- `OnAuthenticationFailed`: Replace the raw `Log.Error(...)` call with `audit.AuthenticationFailed(exception)`.

### 6. Add authorization failure audit logging

**File:** `src/MooBank.Infrastructure/Repositories/SecurityRepository.cs` (modify)

Add `IAuditLog audit` to the primary constructor. Before each `throw new NotAuthorisedException(...)`, call `audit.AuthorizationDenied(user, resource, resourceId, policy)` — `user` is already a constructor parameter.

**`AuthorisationExtensions.cs` — no changes.** These are static extension methods with no DI access. Adding an `IAuditLog` parameter would change the signature for a single callsite (`TransactionTools.cs`). Not worth it — the HTTP audit middleware already captures these denials as failed mutating requests with status codes, and the `SecurityRepository` covers the other 5 authorization failure sites with full detail.

### 7. Enhance import audit logging

**File:** `src/MooBank/Services/ImportTransactionsService.cs` (modify)

Add `IAuditLog audit` to the primary constructor. The `User` is available via the `ImportWorkItem.User` property. Replace the ad-hoc `logger.LogInformation(...)` calls:

- Start of import: `audit.ImportStarted(workItem.User, ...)`
- After SaveChanges: `audit.ImportCompleted(workItem.User, ..., transactionCount)`
- Catch block: `audit.ImportFailed(workItem.User, ..., ex)`

Keep the existing `ILogger<ImportTransactionsService>` for operational logging (service lifecycle messages). The audit logger captures the audit-relevant subset with consistent tagging.

### 8. Add audit to key command handlers

Inject `IAuditLog audit` and `User user` into the primary constructors of high-value command handlers, then use the `SaveChangesAsync` overload. Priority handlers:

| Module | Command | Audit action |
|--------|---------|-------------|
| `Modules.Accounts/Commands/Create.cs` | Create account | `"Created", "Account"` |
| `Modules.Accounts/Commands/Update.cs` | Update account | `"Updated", "Account"` |
| `Modules.Transactions/Commands/Create.cs` | Create transaction | `"Created", "Transaction"` |
| `Modules.Tags/Commands/Create.cs` | Create tag | `"Created", "Tag"` |
| `Modules.Tags/Commands/Delete.cs` | Delete tag | `"Deleted", "Tag"` |
| `Modules.Budgets/Commands/Create.cs` | Create budget | `"Created", "Budget"` |
| `Modules.Families/Commands/Create.cs` | Create family | `"Created", "Family"` |

This is not exhaustive — additional handlers can be instrumented incrementally. The HTTP audit middleware provides baseline coverage for anything not explicitly instrumented.

---

## Files Summary

| File | Action | Description |
|------|--------|-------------|
| `src/MooBank/Audit/IAuditLog.cs` | Create | Injectable audit logging interface |
| `src/MooBank/Audit/AuditLog.cs` | Create | Implementation using `ILogger<AuditLog>` |
| `src/MooBank/Audit/UnitOfWorkAuditExtensions.cs` | Create | `SaveChangesAsync` overload with audit |
| `src/MooBank/ServiceCollectionExtensions.cs` | Modify | Register `IAuditLog` in DI |
| `src/MooBank.Api/Middleware/AuditMiddleware.cs` | Create | HTTP mutation audit middleware |
| `src/MooBank.Api/Program.cs` | Modify | Register audit middleware |
| `src/MooBank.Api/IServiceCollectionExtensions.cs` | Modify | Auth event audit logging |
| `src/MooBank.Infrastructure/Repositories/SecurityRepository.cs` | Modify | Authorization denial audit logging |
| `src/MooBank.Security/Authorisation/AuthorisationExtensions.cs` | No change | Covered by HTTP middleware (static methods, no DI) |
| `src/MooBank/Services/ImportTransactionsService.cs` | Modify | Replace ad-hoc logging with `IAuditLog` |
| `src/MooBank.Modules.*/Commands/*.cs` | Modify | Use `SaveChangesAsync` audit overload (key handlers) |

---

## Verification

1. **Build**: `dotnet build MooBank.slnx` — must compile without warnings
2. **Test**: `dotnet test tests/` — all existing tests must pass. `SecurityRepository` tests may need an `IAuditLog` mock or `NullAuditLog`
3. **Manual verification**:
   - Start the app locally via Aspire AppHost
   - Log in — verify login event appears in Seq with `AuditEvent = true`
   - Create/modify an account — verify both HTTP mutation and DataChanged events appear
   - Import a CSV — verify import started/completed events appear
   - Attempt to access another user's instrument — verify authorization denied event appears
4. **Seq query**: `AuditEvent = true` should return all audit entries; `AuditCategory = "Import"` should filter to import events only

---

## Future Considerations

- **Database audit table**: If in-app audit viewing or compliance reporting is needed, add a second `IAuditLog` implementation (decorator or composite) that writes to an `AuditLog` table. The interface doesn't change.
- **Sensitive data masking**: If audit logs are ever exposed to non-admin users, mask email addresses and IP addresses.
- **Retention policy**: Configure Seq retention to auto-expire audit events after an appropriate period (e.g. 12 months).
