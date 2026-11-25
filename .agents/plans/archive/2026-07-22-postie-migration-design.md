# MooBank → Postie migration and Asm.Cqrs removal — design

Date: 2026-07-22. Approved scope: MooBank now; MooAuth/MooTrack later; both repos land as branch + PR.
This spec is committed with the migration branch (in-repo docs are fine for MooBank).

## Goal

Dogfood Postie 1.0.0 by replacing Asm.Cqrs/Asm.Cqrs.AspNetCore in MooBank, then remove the CQRS
projects from the Asm repo with no functional loss to MooBank.

## Functionality-loss ledger (verified against both codebases)

**Drop-in identical:** `IQuery<T>`, `ICommand<T>`, `ICommand`, `IQueryHandler<,>`,
`ICommandHandler<,>`/`<>`, `IQueryDispatcher.Dispatch`, `ICommandDispatcher.Dispatch`/`Execute`,
`AddCommandHandlers(Assembly)`, `AddQueryHandlers(Assembly)` — same names, same ValueTask
signatures in Postie. MCP tool classes injecting `IQueryDispatcher` need only the namespace change.

**Renames:** `MapDelete` → `MapDeleteCommand` (14 sites); `CommandBinding` → `RequestBinding`
(`None` → `Default`); namespaces `Asm.Cqrs.*` → `Postie.Cqrs.*`, endpoint extensions
`Asm.AspNetCore` → `Postie.AspNetCore`.

**Behavioral deltas:**
1. Command binding default: Asm = `Parameters` for all verbs; Postie = `Body` for POST/PUT/PATCH.
   Rule: every command mapping that relied on Asm's default gets an explicit
   `RequestBinding.Parameters`. Behaviour-preserving; revisit per endpoint later if desired.
2. `MapQuery` null result: Asm = 200 + null body; Postie = 404. Non-issue in MooBank: handlers
   throw `NotFoundException` (53 files) via `Asm.AspNetCore.Api` middleware, which stays.
3. Asm's obsolete void `Dispatch(ICommand)` alias does not exist in Postie; MooBank already uses
   `Execute`.

**Real gap:** `MapPagedQuery<TQuery,TItem>` (6 call sites: writes `X-Total-Count` header, returns
unwrapped `PagedResult<T>.Results`). `PagedResult<T>` lives in the surviving base `Asm` package.
Fix: a new Asm package — working name `Asm.AspNetCore.Postie` — that extends Postie's mapping with
Asm conventions, starting with `MapPagedQuery` (same name, same wire contract: `X-Total-Count` +
unwrapped `Results`, `Produces` matching today's OpenAPI output). Built on Postie's
`IEndpointDispatcher` so the extension is mediator-agnostic like Postie's own mapping layer.
References: `Asm` (PagedResult) + `Postie.AspNetCore` only. Keeps paging reusable for
MooAuth/MooTrack; still a candidate for promotion into Postie itself if the shape proves general.

**Lost but unused by MooBank:** `CommandQueryController` MVC base class (lives in
Asm.Cqrs.AspNetCore). Check MooAuth/MooTrack for usage before their migrations.

**Survives unchanged (not part of the removal):** `WithValidation<T>()` (Asm.AspNetCore
RouteHandlerBuilder extension — chains onto Postie builders), `EndpointGroupBase`, `.WithNames`,
`.ToMachine()`, `PagedResult<T>`, `NotFoundException` + `AddAsmExceptionHandler`/
`UseStandardExceptionHandler` (Asm.AspNetCore.Api), FluentValidation registration via
`AddValidatorsFromAssembly`.

## MooBank migration (branch `feature/postie`, PR)

1. `Directory.Packages.props`: remove `Asm.Cqrs.AspNetCore` 4.0.31; add `Postie.Cqrs.AspNetCore`
   and `Postie.AspNetCore` 1.0.0. Other Asm packages stay at 4.0.31.
2. 15 module csprojs: `Asm.Cqrs.AspNetCore` PackageReference → `Postie.Cqrs.AspNetCore` +
   `Postie.AspNetCore`.
3. 15 `Properties/Global.cs`: `global using Asm.Cqrs.Commands/Queries` →
   `Postie.Cqrs.Commands/Queries`. The 137 request/handler files then compile unchanged.
4. Endpoint files (44): using/namespace adjustments; `MapDelete*` → `MapDeleteCommand*`;
   `CommandBinding.X` → `RequestBinding.X`; add explicit `binding: RequestBinding.Parameters` to
   every command mapping that previously relied on Asm's default; `MapPagedQuery` call sites move
   to the local helper (same call shape).
5. Add `Asm.AspNetCore.Postie` PackageReference where paged endpoints live; `MapPagedQuery` call
   sites keep their call shape.
6. Tests: `MooBank.Modules.Budgets.Tests/Support/TestMocks.cs` re-points its
   `Mock<ICommandDispatcher>` using to Postie; all other test churn is transitive-compile only.
7. Verification gate: full solution build zero warnings; all module test suites green; runtime
   smoke pass comparing pre/post behaviour for one endpoint of each shape — GET by id, paged GET
   (assert `X-Total-Count` + body shape), POST-create (201 + Location), PATCH, PUT route-bound
   command, DELETE void, custom-status `MapCommand` (204/202), the `IFormFile` raw-`MapPost`
   escape hatch, one MCP tool call, one `WithValidation` 400.

## Asm changes (two PRs)

**PR 1 — additive (branch `feature/postie-extensions`, before the MooBank migration):**
1. New project `src/Asm.AspNetCore.Postie`: `MapPagedQuery<TQuery,TItem>` on
   `IEndpointRouteBuilder`, dispatching via Postie's `IEndpointDispatcher`, writing
   `X-Total-Count`, returning `Results.Ok(result.Results)`; XML docs + tests following Asm repo
   conventions. References `Asm` + `Postie.AspNetCore` (Postie 1.0.0 from nuget.org).
2. Ship as a minor release (4.1.x) — no breaking change; MooBank's migration consumes it.

**PR 2 — removal (branch `feature/remove-cqrs`, after the MooBank PR is validated):**
1. Delete `src/Asm.Cqrs` and `src/Asm.Cqrs.AspNetCore` (verified: nothing else in the repo
   references them — `CommandQueryController` lives inside Asm.Cqrs.AspNetCore; before
   MooAuth/MooTrack migrate, check whether either uses it and if so re-home it in
   `Asm.AspNetCore.Postie` on Postie dispatchers).
2. Remove from solution and from CI build/pack lists.
3. Major version bump (→ 5.0.0) so MooAuth/MooTrack stay pinned to the last Cqrs-bearing 4.x
   until their own migrations.
4. Release notes: removal notice + pointer to Postie, `Asm.AspNetCore.Postie`, and the MooBank
   migration recipe (renames, binding rule).

## Sequencing

Asm PR 1 (additive extension package, 4.1.x) → MooBank PR (consumes it, smoke-validated, merged)
→ Asm PR 2 (removal, 5.0.0). The removal ships only after the replacement is proven in real code.

## Out of scope

MooAuth and MooTrack migrations (each gets this document as its template, plus a
`CommandQueryController` usage check); any Postie feature additions (paging lives in
`Asm.AspNetCore.Postie` until proven general enough to graduate); changing per-endpoint binding
semantics beyond behaviour preservation.
