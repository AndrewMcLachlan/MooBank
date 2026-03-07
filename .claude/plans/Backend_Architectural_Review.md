# MooBank Backend Architectural Review

**Date:** December 2024
**Scope:** Backend architecture, code quality, security, and test coverage

---

## Executive Summary

MooBank demonstrates a well-designed architecture using CQRS, DDD, and modular patterns with .NET 10. The codebase follows established patterns consistently and leverages modern ASP.NET Core features. However, several areas require attention ranging from critical runtime issues to maintainability improvements.

### Architecture Strengths

| Aspect | Implementation |
|--------|---------------|
| **CQRS Pattern** | Clean separation of commands/queries with handlers |
| **Modular Design** | 19 self-contained modules with clear boundaries |
| **DDD** | Rich domain entities, specifications, domain events |
| **Authentication** | Azure AD OAuth with JWT Bearer tokens |
| **Authorization** | Policy-based with custom handlers |
| **Database Access** | EF Core with proper Unit of Work pattern |
| **API Design** | Minimal APIs with OpenAPI documentation |
| **External Integrations** | Well-isolated in separate projects |

---

## Issues and Improvements

### Critical (Must Fix)

#### 1. Thread Safety Violation in RunRulesService

**Location:** `src/Asm.MooBank/Services/RunRulesService.cs:37-47`

**Problem:** `Parallel.ForEach` is used to modify EF Core entities, which violates DbContext thread safety. Entity modifications in parallel can cause data corruption or runtime exceptions.

**Current Code:**
```csharp
Parallel.ForEach(transactions, (transaction) => {
    transaction.AddOrUpdateSplit(...);
});
```

**Fix:** Replace with sequential `foreach` or use separate DbContext instances per thread via `IDbContextFactory<T>`.

**Effort:** Low
**Risk if not fixed:** Data corruption, runtime exceptions

---

#### 2. N+1 Query Problem in ForecastEngine

**Location:** `src/Asm.MooBank.Modules.Forecast/Services/ForecastEngine.cs`
- Lines 115-130: `FilterAccountsForHistoricalAnalysis()`
- Lines 159-173: `CalculateHistoricalStartingBalance()`
- Lines 187-202: `CalculateBaselineOutgoings()`
- Lines 219-230: Additional per-account queries

**Problem:** Multiple `foreach` loops make individual database calls per account, causing significant performance degradation with many accounts.

**Fix:**
- Batch account IDs and use `WHERE Id IN (...)` queries
- Load all required data upfront with eager loading specifications
- Consider caching account data for the duration of calculation

**Effort:** Medium
**Risk if not fixed:** Poor performance, database load

---

#### 3. SemaphoreSlim Resource Leak

**Locations:**
- `src/Asm.MooBank/Services/ReprocessTransactionsService.cs:67`
- `src/Asm.MooBank/Services/RunRulesService.cs:71`

**Problem:** `SemaphoreSlim` instances are created but never disposed, causing potential resource leaks in long-running services.

**Fix:** Implement `IAsyncDisposable` on both services and dispose semaphores in `DisposeAsync()`.

**Effort:** Low
**Risk if not fixed:** Memory leaks in production

---

#### 4. Missing Rate Limiting

**Not missing, handled by hosting**

---

### High Priority (Should Fix)

#### 5. Silent Exception Swallowing

**Location:** `src/Asm.MooBank.Modules.Forecast/Services/ForecastEngine.cs`
- Lines 126-129, 169-172, 198-201, 315-318

**Problem:** Generic `catch (Exception)` blocks with comments like "Skip accounts with errors" make debugging nearly impossible.

**Fix:**
- Log exceptions with full context (account ID, operation name)
- Consider aggregating errors and reporting them to the caller
- Use specific exception types where possible

**Effort:** Low
**Risk if not fixed:** Silent failures, difficult debugging

---

#### 6. API Keys in URL Query Strings

**Already fixed**

---

#### 7. Missing Cancellation Token Propagation

**Locations:**
- `src/Asm.MooBank/Services/RecurringTransactions.cs:27-46`
- Various other async methods

**Problem:** Cancellation tokens are not passed through async chains, preventing graceful shutdown.

**Fix:**
- Add `CancellationToken` parameter to `Process()` method
- Propagate token to all async calls including `SaveChangesAsync(cancellationToken)`

**Effort:** Low
**Risk if not fixed:** Ungraceful shutdown, resource cleanup issues

---

#### 8. Inconsistent Authorization Patterns

**Problem:** Mixed authorization approaches:
- Some endpoints: `Policies.GetInstrumentViewerPolicy("instrumentId")` (parameterized)
- Other endpoints: `Policies.InstrumentViewer` (direct constant)
- Some handlers also call `AuthorizeAsync()`, duplicating endpoint-level checks

**Fix:**
- Standardize on parameterized policy approach for resource-based authorization
- Remove duplicate authorization checks in handlers
- Document the authorization pattern in CLAUDE.md

**Effort:** Medium
**Risk if not fixed:** Maintenance confusion, potential security gaps

---

#### 9. Brittle HTTP Client Error Handling

**Locations:**
- `src/Asm.MooBank.Abs/AbsClient.cs:30-57`
- `src/Asm.MooBank.ExchangeRateApi/ExchangeRateClient.cs:30-34`
- `src/Asm.MooBank.Eodhd/StockPriceClient.cs:39-43`

**Problem:** Generic `catch (Exception ex)` returns null/empty without distinguishing transient vs permanent failures.

**Fix:**
- Add Polly retry policies for transient failures
- Implement circuit breaker pattern
- Differentiate between `HttpRequestException`, `TaskCanceledException`, and other exceptions
- Return `Result<T>` type instead of null

**Effort:** Medium
**Risk if not fixed:** Poor reliability, silent failures

---

### Medium Priority (Should Improve)

#### 10. Large Methods Need Refactoring

| File | Lines | Recommendation |
|------|-------|----------------|
| `ForecastEngine.cs` | 526 | Extract calculation methods into separate services |
| `IngImporter.cs` | 229 | Extract CSV parsing to dedicated parser class |
| `MacquarieImporter.cs` | 209 | Apply same pattern as ING |

**Fix:** Apply Single Responsibility Principle, extract into smaller testable units.

**Effort:** Medium
**Risk if not fixed:** Maintainability issues

---

#### 11. Missing Input Validation

**Problem:** FluentValidation is referenced but no validator classes exist. String fields lack:
- Maximum length constraints
- Pattern validation (email, phone, etc.)
- Whitespace handling

**Fix:** Implement `AbstractValidator<T>` classes for all command models:

```csharp
public class CreateAccountValidator : AbstractValidator<Create>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
```

**Effort:** Medium
**Risk if not fixed:** Data quality issues, potential security vulnerabilities

---

#### 12. Insufficient Test Coverage

**Current State:**

| Coverage Area | Status |
|---------------|--------|
| Modules tested | 1 of 19 (Accounts only) |
| Domain entities | No unit tests |
| Repository layer | No tests |
| External integrations | No tests |
| Estimated coverage | 5-10% |

**Fix:** Implement test coverage in phases:

1. **Phase 1:** Domain entity unit tests (business logic validation)
2. **Phase 2:** Expand BDD tests to Transactions, Budgets, Forecast modules
3. **Phase 3:** Integration tests for repositories
4. **Phase 4:** External service integration tests with mocks

**Effort:** High
**Risk if not fixed:** Regression bugs, difficult refactoring

---

#### 13. Sync-over-Async Pattern

**Location:** `src/Asm.MooBank.Security/ClaimsUserDataProvider.cs:11-12`

**Problem:**
```csharp
public Task<User> GetCurrentUserAsync() => Task.FromResult(GetCurrentUser());
```

This creates unnecessary Task allocations.

**Fix:** Remove the async code, it's not used.

**Effort:** Low
**Risk if not fixed:** None, it's not used.

---

#### 14. CORS Not Explicitly Configured

**Problem:** No `AddCors()` or `UseCors()` calls found. While the default is restrictive, explicit configuration is preferred for clarity and maintainability.

**Fix:** Do nothing. Effort is not low (configuration per environment, either hard-coded or through new keys) for 0 advantage.

**Effort:** Medium
**Risk if not fixed:** None whatsoever. The default (same origin) is correct and secure.

---

### Low Priority (Nice to Have)

#### 15. Inconsistent Specification Usage

**Problem:** Some queries use `new IncludeSplitsSpecification()`, others use `.IncludeAll()` extension method.

**Fix:** Standardize on specification pattern throughout codebase.

**Effort:** Low

---

#### 16. Hard-coded Magic Numbers

**Problem:** Column indices in importers, date formats in clients are not constants.

**Fix:** Extract to named constants for clarity.

**Effort:** Low

---

#### 17. Security Headers Audit

**Problem:** No problem, this works just fine.

---

## Implementation Plan

### Phase 1: Critical Fixes (1-2 days)

| # | Task | File(s) | Effort |
|---|------|---------|--------|
| 1 | Fix `Parallel.ForEach` thread safety | `RunRulesService.cs` | 1 hour |
| 2 | Add rate limiting middleware | `Program.cs` | 2 hours |
| 3 | Implement `IAsyncDisposable` for semaphores | `ReprocessTransactionsService.cs`, `RunRulesService.cs` | 1 hour |
| 4 | Batch database calls in ForecastEngine | `ForecastEngine.cs` | 4 hours |

### Phase 2: Security & Reliability (2-3 days)

| # | Task | File(s) | Effort |
|---|------|---------|--------|
| 5 | Add Polly retry policies | `AbsClient.cs`, `ExchangeRateClient.cs`, `StockPriceClient.cs` | 3 hours |
| 6 | Move API keys to headers | `ExchangeRateClient.cs`, `StockPriceClient.cs` | 1 hour |
| 7 | Add cancellation token propagation | Multiple services | 2 hours |
| 8 | Implement FluentValidation validators | New validator classes | 4 hours |
| 9 | Add explicit CORS configuration | `Program.cs` | 1 hour |

### Phase 3: Maintainability (3-5 days)

| # | Task | File(s) | Effort |
|---|------|---------|--------|
| 10 | Refactor ForecastEngine into smaller services | `ForecastEngine.cs` | 6 hours |
| 11 | Refactor importers with parser extraction | `IngImporter.cs`, `MacquarieImporter.cs` | 4 hours |
| 12 | Standardize authorization patterns | Multiple endpoint files | 3 hours |
| 13 | Improve exception handling with context | `ForecastEngine.cs`, clients | 2 hours |

### Phase 4: Test Coverage (Ongoing)

| # | Task | Scope | Effort |
|---|------|-------|--------|
| 14 | Domain entity unit tests | `Asm.MooBank.Domain` | 8 hours |
| 15 | BDD tests for Transactions module | `Asm.MooBank.Modules.Transactions` | 6 hours |
| 16 | BDD tests for Budgets module | `Asm.MooBank.Modules.Budgets` | 4 hours |
| 17 | BDD tests for Forecast module | `Asm.MooBank.Modules.Forecast` | 6 hours |
| 18 | Integration tests for repositories | `Asm.MooBank.Infrastructure` | 8 hours |

---

## Success Criteria

- [ ] All critical issues resolved
- [ ] Rate limiting in place
- [ ] No thread safety violations
- [ ] N+1 queries eliminated from ForecastEngine
- [ ] HTTP clients have retry policies
- [ ] Input validation on all commands
- [ ] Test coverage > 30%

---

## References

- [CLAUDE.md](../CLAUDE.md) - Architecture guidelines
- [ASM Library](https://github.com/AndrewMcLachlan/ASM) - Custom infrastructure library
- [Microsoft Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit) - Rate limiting documentation
- [Polly](https://github.com/App-vNext/Polly) - Resilience library for .NET
