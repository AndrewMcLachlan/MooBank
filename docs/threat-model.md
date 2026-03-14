# MooBank Threat Model

**Date:** 2026-03-12
**Version:** 1.0
**Scope:** Authentication, authorization, data isolation, file upload, API security

---

## 1. Executive Summary

This document provides a threat model for the MooBank personal finance management application. It follows the STRIDE methodology and maps threats to specific code patterns found in the codebase. Each threat includes an assessment of existing mitigations, residual risk, and recommended improvements.

**Application Profile:**
- Browser-based SPA (React/TypeScript) communicating with an ASP.NET Core Minimal API
- Authentication via Azure AD (OAuth 2.0 / OIDC) with JWT bearer tokens
- Azure SQL Database with Entity Framework Core
- Handles sensitive personal financial data (account balances, transactions, spending patterns)
- Multi-tenant via Family-based data isolation
- File upload capability (CSV transaction import)
- MCP server endpoint for AI tool integration

---

## 2. System Architecture Overview

```
                    Internet
                       |
            [Azure App Service]
                       |
        +-----------------------------+
        |     ASP.NET Core API        |
        |  (JWT Auth + HSTS + TLS)    |
        |                             |
        |  +-------+  +-----------+   |
        |  | Auth  |  | Modules   |   |
        |  | Midw. |  | (CQRS)    |   |
        |  +-------+  +-----------+   |
        |       |           |         |
        |  +--------------------+     |
        |  | EF Core / Repos    |     |
        |  +--------------------+     |
        |       |                     |
        +-------|---------------------+
                |
        [Azure SQL Database]
```

**Trust Boundaries:**
1. Browser <-> API Server (Internet boundary)
2. API Server <-> Azure SQL Database (Internal network)
3. API Server <-> External APIs (ExchangeRateApi, EODHD, ABS)
4. Authenticated user <-> Other users' data (Logical boundary)
5. Regular user <-> Admin functionality (Role boundary)

---

## 3. Assets

| ID | Asset | Sensitivity | Location |
|----|-------|-------------|----------|
| A-1 | Azure AD JWT tokens | **Critical** | Browser (short-lived), API server (validation) |
| A-2 | Financial data (balances, transactions) | **High** | Azure SQL Database, API responses |
| A-3 | User spending patterns and budgets | **High** | Azure SQL Database, API responses |
| A-4 | CSV import files | Medium | In-memory during processing, raw data in DB |
| A-5 | User identity and family membership | Medium | Azure AD, claims cache, database |
| A-6 | API keys for external services | **Critical** | Azure Key Vault, appsettings |
| A-7 | Database connection strings | **Critical** | Azure Key Vault, appsettings |

---

## 4. Threat Analysis (STRIDE)

### 4.1 Spoofing (Identity)

#### T-S1: JWT Token Theft or Replay

**Risk Level:** Medium
**Description:** An attacker intercepts or steals a valid JWT token to impersonate a legitimate user.

**Existing Mitigations:**
- Azure AD issues short-lived tokens with standard JWT validation
- HSTS enforcement in production (`src/MooBank.Api/Program.cs`, line 109-113):
  ```csharp
  services.AddHsts(options =>
  {
      options.MaxAge = TimeSpan.FromDays(365);
      options.IncludeSubDomains = true;
  });
  ```
- HTTPS redirection in non-development environments (line 201)
- `UseSecurityHeaders()` middleware applied (line 218)

**Residual Risk:** Low. Azure AD's short-lived tokens, HSTS, and Cloudflare TLS termination make interception impractical under normal operation.

**Recommended Improvements:**
- Consider adding token binding or sender-constrained tokens if supported by Azure AD configuration
- Ensure CORS is properly configured in production (currently only defined in `appsettings.Development.json`)

---

### 4.2 Tampering (Data Integrity)

#### T-T1: CSV Import File Manipulation

**Risk Level:** Medium
**Description:** The CSV import functionality (`src/MooBank.Institution.Ing/Importers/IngImporter.cs`) manually parses CSV files using `String.Split(",")` (line 55). While this is for importing financial data the user owns, a maliciously crafted CSV could potentially cause unexpected behavior.

**Existing Mitigations:**
- Column count validation (line 78-82)
- Date format validation (line 84-88)
- Numeric parsing validation for credit/debit/balance columns
- Duplicate transaction detection (lines 123-129)
- Import is processed through a background queue, isolating it from the request context
- Authorization requires instrument viewer/owner access
- 10MB request size limit via `RequestSizeLimitAttribute`
- File type validation (`.csv` extension and `text/csv` content type)

**Residual Risk:** Low. Multiple layers of validation are in place. The user is importing their own data, limiting the motivation for attack.

**Recommended Improvements:**
- The anti-forgery token is explicitly disabled for the import endpoint (`src/MooBank.Modules.Instruments/Endpoints/Import.cs`, line 27): `.WithMetadata(new RequireAntiforgeryTokenAttribute(false))` -- document the rationale or consider re-enabling
- Consider using CsvHelper (already a dependency in the project) consistently instead of manual CSV parsing for the ING importer
- Add a maximum line count limit to prevent processing of excessively large files
- Sanitize the description field before storing to prevent stored XSS if values are rendered unescaped

#### T-T2: Claims Cache Poisoning

**Risk Level:** Low
**Description:** User claims (account IDs, family ID, groups) are cached for 5 minutes via `HybridCache` (`src/MooBank.Api/IServiceCollectionExtensions.cs`, lines 15-18, 40-91). If a user's permissions change (e.g., removed from a family, account unshared), the cached claims remain valid for up to 5 minutes.

**Existing Mitigations:**
- Cache expiration is set to 5 minutes
- Claims are regenerated on next token validation after cache expiry

**Risk Assessment:**
The 5-minute window is narrow, and permission changes are rare admin-level operations in a personal finance app. The complexity of implementing cache invalidation likely outweighs the risk. This is an acceptable risk for now.

**Recommended Improvements:**
- Provide a mechanism to invalidate the claims cache when permissions change (e.g., when a family member is removed)
- Consider reducing cache duration or implementing cache-aside pattern with event-driven invalidation

---

### 4.3 Repudiation (Non-Repudiation)

#### T-R1: Insufficient Audit Logging

**Risk Level:** Medium
**Description:** There is no comprehensive audit trail for security-relevant actions. The only logging observed is authentication failure logging (`Log.Error(context.Exception, "Authentication failed.")`) and importer-level informational logging. Transaction imports are queued for background processing without a persistent audit record of who initiated the import, though raw transaction data is stored with an `Imported` timestamp.

**Existing Mitigations:**
- Authentication failures are logged via Serilog
- Application Insights integration is configured
- Serilog sinks include File and Seq
- Raw import data is stored in institution-specific tables (e.g., `ing.TransactionRaw`) with an `Imported` timestamp

**Residual Risk:** Medium. Lack of audit trails limits the ability to investigate suspicious activity or reconstruct events after an incident.

**Recommended Improvements:**
- Add structured logging for security-relevant events:
  - Successful logins (especially first-time / new user provisioning)
  - Failed authorization attempts
  - Data modification operations (create/update/delete on financial instruments)
  - CSV import operations (who imported, how many transactions, which instrument)
  - Admin operations (family creation, institution management)
- Consider implementing a dedicated audit trail table for compliance-relevant actions
- Log the user ID and IP address for all write operations

---

### 4.4 Denial of Service

#### T-D1: No Application-Level Rate Limiting

**Risk Level:** High
**Status:** MITIGATED (infrastructure)

**Description:** No rate limiting middleware is configured in the application code.

**Mitigations Applied:**
- Application is hosted behind Cloudflare, which provides edge-level rate limiting, DDoS protection, and bot mitigation
- Authentication is required for all endpoints (limits attack surface to authenticated users)

**Residual Risk:** Low. Cloudflare provides comprehensive edge-level protection.

**Recommended Improvements:**
- Ensure Cloudflare rate limiting rules are configured appropriately for the API endpoints
- Consider application-level rate limiting for expensive authenticated operations (CSV import, forecast execution) if Cloudflare rules are insufficient for per-user throttling

#### T-D2: Unbounded File Upload

**Status:** PARTIALLY MITIGATED

**Risk Level:** Medium → Low
**Description:** The CSV import endpoint (`src/MooBank.Modules.Instruments/Endpoints/Import.cs`) accepts file uploads via `IFormFile`. The file is read entirely into a `MemoryStream` (`src/MooBank.Modules.Instruments/Commands/Import/Import.cs`, lines 16-18):
```csharp
using var memoryStream = new MemoryStream();
await stream.CopyToAsync(memoryStream, cancellationToken);
var fileData = memoryStream.ToArray();
```

**Mitigations Applied:**
- Explicit 10MB request size limit added to the import endpoint via `RequestSizeLimitAttribute`
- File type validation (`.csv` extension and `text/csv` content type) rejects non-CSV uploads
- Authentication required (fallback policy + explicit `.RequireAuthorization()`)
- Import is queued for background processing, which limits direct request impact

**Residual Risk:** Low. The 10MB size limit, file type validation, and authentication requirement significantly reduce the attack surface. Memory impact is bounded and short-lived.

**Recommended Improvements:**
- Consider streaming the file to blob storage instead of loading entirely into memory

#### T-D3: Report Stored Procedures Without Pagination

**Risk Level:** Low
**Description:** Report stored procedures (`src/MooBank.Infrastructure/Repositories/ReportRepository.cs`) execute against potentially large date ranges without pagination. The `GetCreditDebitTotalsForAccounts` and `GetMonthlyBalancesForAccounts` methods iterate over multiple accounts sequentially.

**Existing Mitigations:**
- Reports are scoped to specific accounts/groups via authorization
- Date range parameters constrain result sets
- Stored procedures use temp tables with indexes for performance

**Residual Risk:** Low. Reports are scoped to the user's own accounts and constrained by date range parameters. The indexed temp tables in stored procedures provide adequate performance for typical usage.

**Recommended Improvements:**
- Add maximum date range validation (e.g., max 5 years)
- Consider caching report results for frequently requested date ranges
- Add query timeout configuration for report endpoints

---

## 5. Data Flow Threat Analysis

### 5.1 Authentication Flow

```
Browser -> Azure AD -> JWT Token -> API Server -> Token Validation -> Claims Enrichment -> Cache
```

**Threats:** T-S1, T-T2
**Key Code:** `src/MooBank.Api/IServiceCollectionExtensions.cs`

### 5.2 Transaction Import Flow

```
Browser -> File Upload -> API Endpoint -> Memory Stream -> Background Queue -> CSV Parser -> Database
```

**Threats:** T-T1, T-D2, T-R1
**Key Code:** `src/MooBank.Modules.Instruments/Endpoints/Import.cs`, `src/MooBank.Institution.Ing/Importers/IngImporter.cs`

### 5.3 Report Generation Flow

```
Browser -> API Endpoint -> Authorization Check -> Stored Procedure -> Database -> Response
```

**Threats:** T-D3
**Key Code:** `src/MooBank.Infrastructure/Repositories/ReportRepository.cs`

### 5.4 Data Access Flow

```
API Request -> Module Endpoint -> Auth Policy Check -> CQRS Handler -> EF Core Query (FamilyId filter) -> Database
```

**Threats:** (none remaining)
**Key Code:** Various module endpoint and query handler files

---

## 6. SQL Injection Assessment

**Risk Level:** Low

The application uses Entity Framework Core throughout, which parameterizes all queries automatically. The stored procedure calls in `ReportRepository.cs` use `FromSqlInterpolated()` which properly parameterizes interpolated values:

```csharp
// Safe - FromSqlInterpolated parameterizes automatically
await mooBankContext.TransactionTagTotals.FromSqlInterpolated(
    $@"EXEC dbo.GetTransactionTotalsByTag {accountId}, {startDate}, {endDate}, {rootTagId}, {(int)filterType}"
).AsNoTracking().ToListAsync(cancellationToken);
```

No instances of `FromSqlRaw` or `ExecuteSqlRaw` with string concatenation were found in the codebase.

---

## 7. Cross-Site Scripting (XSS) Assessment

**Risk Level:** Low

- The frontend is a React SPA which auto-escapes output by default
- `UseSecurityHeaders()` middleware is applied (likely includes `Content-Security-Policy`, `X-Content-Type-Options`, `X-Frame-Options` -- would need to verify the ASM library implementation)
- Transaction descriptions from CSV imports are stored as-is; ensure they are not rendered with `dangerouslySetInnerHTML` on the frontend

---

## 8. CORS Assessment

**Risk Level:** Low

- CORS configuration is only present in `appsettings.Development.json` with specific allowed origins:
  ```json
  "CORS": {
      "AllowedOrigins": ["https://localhost:7005", "http://localhost:5005", "https://localhost:3005"]
  }
  ```
- No `UseCors()` call was found in the application code -- CORS may be handled by the ASM library's `WebApplicationStart.Run()` method or by Azure App Service configuration
- Production CORS configuration should be verified at the infrastructure level

---

## 9. Positive Security Observations

The following security practices are already well-implemented:

- **Fallback authentication policy**: All endpoints require authentication by default — no endpoint can accidentally be left unprotected
- **Defense-in-depth authorization**: Route-parameter policies on endpoints + family-scoped filtering in CQRS handlers provide two independent layers of access control
- **Family-based data isolation**: All queries filter by `FamilyId`, preventing cross-tenant data access at the data layer
- **Resource-based MCP authorization**: MCP tools use `IAuthorizationService` with the same requirement/handler infrastructure as REST endpoints
- **FluentValidation on all modules**: Input validation is consistently applied across Assets, Stocks, Bills, Budgets, Forecast, and Users modules
- **Parameterized database access**: Entity Framework Core and `FromSqlInterpolated()` eliminate SQL injection risk
- **React auto-escaping**: Frontend framework prevents XSS by default
- **CSV import defense-in-depth**: File type validation, size limits, column count validation, date/numeric parsing, duplicate detection, and background processing
- **Short-lived tokens**: Azure AD issues tokens with limited lifetime, reducing the window for token theft

---

## 10. Threat Summary Matrix

| ID | Threat | Category | Risk | Existing Mitigation | Action Required |
|----|--------|----------|------|---------------------|-----------------|
| T-S1 | JWT Token Theft | Spoofing | Medium | HSTS, TLS | Verify security headers |
| T-T1 | CSV Import Manipulation | Tampering | Medium | Input validation, size limit, file type check | Review anti-forgery token |
| T-T2 | Claims Cache Staleness | Tampering | Low | 5-min expiry | Add cache invalidation |
| T-R1 | Insufficient Audit Logging | Repudiation | Medium | Basic logging, raw import data stored | Add comprehensive audit trail |
| T-D1 | No Rate Limiting | DoS | High | **MITIGATED** -- Cloudflare edge rate limiting + DDoS protection | Review Cloudflare rules |
| T-D2 | Unbounded File Upload | DoS | Medium → Low | **PARTIALLY MITIGATED** -- 10MB limit, file type validation, auth required | Consider blob storage streaming |
| T-D3 | Report Query Performance | DoS | Low | Date range params | Add max date range validation |

---

## 11. Priority Recommendations

### Short-Term (Medium Priority)

1. **Implement audit logging** for security-relevant operations (logins, data modifications, imports).

### Long-Term (Low Priority)

2. **Review and document the security headers** applied by `UseSecurityHeaders()` from the ASM library.

---

## 12. Methodology Notes

This threat model was produced by analyzing the MooBank codebase statically. Key files examined:

- **Authentication/Authorization:** `src/MooBank.Security/` (all files), `src/MooBank.Api/Program.cs`, `src/MooBank.Api/IServiceCollectionExtensions.cs`
- **Endpoint Definitions:** All `Module.cs` and `Endpoints/*.cs` files across 15 modules
- **Data Access:** `src/MooBank.Infrastructure/Repositories/ReportRepository.cs`, `src/MooBank.Infrastructure/MooBankContext.cs`
- **CSV Import:** `src/MooBank.Institution.Ing/Importers/IngImporter.cs`, `src/MooBank.Modules.Instruments/Endpoints/Import.cs`
- **Configuration:** `appsettings.json`, `appsettings.Development.json`
- **Query Handlers:** Selected query handlers across Tags, Accounts, Budgets, Forecasts modules to verify family-scoped filtering
- **Database:** Stored procedures in `src/MooBank.Database/dbo/StoredProcedures/`

The ASM library (`Asm.AspNetCore.*`) is an external dependency whose internals were not fully auditable. Threats related to `UseSecurityHeaders()`, `UseStandardExceptionHandler()`, and `WebApplicationStart.Run()` assume standard secure implementations but should be verified against the ASM library source.
