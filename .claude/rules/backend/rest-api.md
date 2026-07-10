---
paths:
  - "src/MooBank.Web.Api/**"
  - "src/MooBank.Modules*/Endpoints/**"
---

# REST API Design & Authorization

## API Design Principles

### RESTful Conventions
- Use RESTful conventions for endpoint URLs
- Group endpoints logically (e.g., `/api/accounts/{id}/virtual`, `/api/instruments/{id}/import`)
- Use proper HTTP verbs:
  - `GET` - Read operations
  - `POST` - Create operations
  - `PATCH` - Partial updates
  - `PUT` - Full updates (rarely used)
  - `DELETE` - Delete operations

### Status Codes
- `200 OK` - Successful read/update
- `201 Created` - Successful creation
- `204 No Content` - Successful deletion
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource doesn't exist

## OpenAPI Documentation

- The project uses `Microsoft.AspNetCore.OpenApi` (not Swashbuckle) for OpenAPI document generation
- OpenAPI documents are generated at build time via `Microsoft.Extensions.ApiDescription.Server`
- **Do not add `Swashbuckle.AspNetCore` as it conflicts with build-time generation**
- Security schemes (OIDC) must handle null configuration gracefully for build-time generation
- Do not use `.WithOpenApi()` as this is deprecated in .NET 10

## Authorization

### Policy-Based Authorization
- Policies are defined in `MooBank.Security`
- Applied at the endpoint level using `.RequireAuthorization()`

### Instrument Authorization Pattern

When applying authorization to endpoints that involve instruments, **always use parameterized/dynamic policies** that extract the instrument ID from the route parameter.

**Always use:**
```csharp
// Dynamic policy - extracts instrumentId from route and validates ownership/access
.RequireAuthorization(Policies.GetInstrumentViewerPolicy("instrumentId"));
.RequireAuthorization(Policies.GetInstrumentOwnerPolicy("instrumentId"));
```

**Never use static policies for instrument-based authorization:**
```csharp
// WRONG - Static policies don't validate against the specific instrument in the route
.RequireAuthorization(Policies.InstrumentViewer);
.RequireAuthorization(Policies.InstrumentOwner);
```

## Multi-tenancy

- Users are grouped into Families for data isolation
- Authorization policies enforce data access boundaries
- Always consider tenant context when designing new endpoints

## Authorization Ownership Contract

Auth is handled by policies and requirements wherever possible; filtering is defence-in-depth.

1. **Single-resource routes** (id in the route) → parameterized policy backed by a route-based
   requirement handler (403 + audit on denial). Existing policies: `GetInstrumentViewerPolicy`,
   `GetInstrumentOwnerPolicy`, `GetGroupOwnerPolicy`, `GetBudgetLinePolicy`, `GetTagFamilyPolicy`,
   `GetForecastPlanPolicy`. A policy-less id route is not acceptable where a suitable requirement exists.
2. **Non-route contexts** (MCP tools, command-body foreign keys such as `GroupId`/`FamilyId`) →
   resource-based requirement invoked via `ISecurity.Assert*` (audits and throws).
3. **Collection/list endpoints** → handler/query filtering by user/family (policies cannot scope
   lists); the EF named query filters (e.g. `"Family"` on Tag) make this structural.
4. **Repository and query filters are defence-in-depth** for id routes and the primary mechanism
   for lists. Redundancy with a policy is acceptable; do not add handler asserts that a policy or
   repository filter already makes unreachable.
5. **Denial auditing**: route-based handlers audit in their base class; resource-based handlers do
   not audit — their `ISecurity` callers do. Every denial is logged exactly once.
6. **Truth sources**: route handlers for instruments/groups answer from cached claims (5-minute
   staleness accepted); data-backed handlers (tag, budget line, forecast plan) and resource
   handlers query the database via `IAuthorisationRepository`.
