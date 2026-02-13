---
paths:
  - "src/Asm.MooBank.Web.Api/**"
  - "src/Asm.MooBank.Modules*/Endpoints/**"
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
- Policies are defined in `Asm.MooBank.Security`
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
