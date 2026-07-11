# MooBank Architectural Review — 2026-07-10

Scope: full solution (backend layout, CQRS/DDD, data access/EF, security/authorization, frontend SPA), reviewed against the house rules in `.claude/rules/**`, `CLAUDE.md`, and `.claude/PRD.md`. Findings are classified as **Violations** (breaks a stated pattern or protection), **Inconsistencies** (two patterns coexist), and **Observations** (trade-offs worth a decision). Settled decisions from prior reviews (Viewer policies stay as-is, specifications stay lean, Macquarie sequence numbers, `strictNullChecks: false`) are respected and not relitigated.

---

## Executive summary

The architecture is fundamentally sound: endpoints are uniformly thin, unit-of-work discipline is flawless (one save per handler, none in domain/infrastructure), context exposure is well contained, cancellation is plumbed end-to-end, and the new ISecurity/IAuthorisationRepository split is clean where applied. The problems are not structural rot — they are **drift**:

1. **The documentation describes a codebase that no longer exists.** `AGENTS.md` actively instructs agents to rebuild the architecture you just migrated away from. The rules docs codify minority patterns. The PRD's headline tech-debt item is already done.
2. **Tenant isolation is enforced at three different layers with no defined owner**, and exactly one handler forgot its layer — producing the review's only true isolation gap (`GetTagTrendReport`).
3. **Names have stopped carrying their meaning**: `[AggregateRoot]` is a DI switch, "Repository" covers read-model gateways, `Delete` means three different things, and one auth base class is named for the wrong noun.
4. **Several deliberate idioms have an accidental twin** (two specification extensions, three mutation-invalidation styles, three command-binding styles, two tenancy-check styles, two event-raising sites).

---

## Theme 1 — Documentation/rules drift (highest leverage, lowest risk)

| Doc | Drift |
|---|---|
| `src/MooBank.Web.App/AGENTS.md` | Instructs React Router 7, `src/services/*Service.ts`, `useApiGet/Post/Patch/Delete`, `src/pages/` — **none exist** (0 usages). An agent following it will reintroduce the pre-migration architecture. |
| `CLAUDE.md` / `.claude/PRD.md` | Document `src/MooBank.Web.Api/` and `MooBank.Web.Jobs` — the entry project is `src/MooBank.Api`, and jobs are WebJobs classes *inside* the API (`src/MooBank.Api/Jobs/*.cs`). PRD also still says React Router 7. |
| `.claude/PRD.md` tech debt | "Replace hand-coded services with ts-openapi react-query hooks" is **done**: zero hand-coded HTTP services remain; ~120 files consume generated `react-query.gen` options; the only deviations are 3 justified `sdk.gen` wrappers and the documented paged-query pattern. |
| `.claude/rules/backend/csharp.md` | "Example: `TransactionAddedEvent` triggers balance updates" — the event is raised (`Transaction.cs:46,79`) but has **no handler anywhere**. "Handled by event handlers in the infrastructure layer" — 4 of 5 handlers live in Domain (`MooBank.Domain/Entities/Transactions/EventHandlers/`, `.../Instrument/EventHandlers/`), which is arguably the *better* home for cross-aggregate domain logic. |
| `.claude/rules/backend/cqrs.md` / `entity-framework.md` | Documents `.Apply()` only (nine files use ASM's `.Specify()`); "navigation properties loaded via specifications" describes the minority — 16 specifications vs 36 query handlers with inline `.Include(...)`. |
| `.claude/rules/frontend/typescript.md:13` | Still says React Router 7. |

**Recommendation:** one docs PR — rewrite `AGENTS.md` around the real architecture (generated hey-api hooks, TanStack Router `-hooks`/`-components` co-location, `transactionKeys.ts` invalidation idiom), fix the layout in CLAUDE.md/PRD, and amend the rules to match settled reality (event handlers may live in Domain for cross-aggregate logic; inline Includes are acceptable in query handlers, specifications for reused bundles; commands return full DTOs deliberately; Transactions module deliberately uses Viewer for writes — document it). Where the rule is right and the code wrong, that's Theme 4.

---

## Theme 2 — Tenant isolation: layered but unowned

Isolation is enforced variously by parameterized route policies, handler-level family/user filters, and (sometimes) repository-level filters. All endpoint groups are registered with some `RequireAuthorization`, and the fallback policy requires an authenticated user — the surface is covered. But which layer *owns* the check is per-module convention:

**Violation (the one real gap):**
- `src/MooBank.Modules.Reports/Queries/GetTagTrendReport.cs:22` — `tags.SingleAsync(t => t.Id == request.TagId)` with a client-supplied route `tagId` and **no `FamilyId` filter**; the response echoes `tag.Name`. The route policy authorizes `accountId` only, so any authenticated user can enumerate integer tag ids and read other families' tag names. Compare `Modules.Tags/Queries/Get.cs:10` which filters correctly. Siblings (`GetSavingsInterestReport.cs:49`, `GetPrincipalVsInterestReport.cs:55`) use server-derived tag ids and are safe today but share the shape.

**Inconsistencies:**
- Two handler-layer idioms, sometimes within one module (Forecast, Tags): inline `p.FamilyId == user.FamilyId` filter (cross-family → 404, no audit) vs fetch-then-`security.AssertFamilyPermission` (→ 403 + audit). Different status codes, disclosure, and audit trail for the same event.
- Dead duplicate checks: `TagRepository.GetById` already filters by family (`TagRepository.cs:35`), so `AssertFamilyPermission` in Tags Update/Delete/AddSubTag/RemoveSubTag can never fail. One layer is dead code.
- Repository-layer filtering is uneven: Tag/LogicalAccount/Instrument/UtilityAccount repositories filter by family/ownership; Transaction/Group/Budget/Rule repositories return any row by id (`RuleRepository.cs:23` explicitly unfiltered) and rely on handlers/policies to compensate.
- Admin is enforced at up to three layers on `/families/admin` (group policy + endpoint policy + handler assert); everywhere else one layer is trusted.
- Denial auditing is uneven: `Security.cs` and `BudgetLineAuthorisationHandler` audit; the instrument/group route handlers and all resource handlers don't. Probing instrument ids is invisible; probing budget lines is logged. The MCP `AssertInstrumentViewer` extension (`AuthorisationExtensions`) doesn't audit either — and duplicates ISecurity's role as "the friendly wrapper".
- GroupOwner requirement has two truth sources: route handler answers from cached claims (`user.Groups`), resource handler queries the DB (`IAuthorisationRepository.IsGroupOwner`) — different answers possible during the 5-minute claims-cache window.

**Observations:**
- `IAuthorisationRepository.GetOwnedInstrumentIds` has zero callers — dead surface from the #862 refactor.
- `Policies.FamilyMember` is registered as a named policy but backed only by a resource-based handler; attaching it to a route would always fail closed. It exists solely to back `AssertFamilyPermission` — the named registration is a trap.
- `GroupOwnerAuthorisationHandler` derives from `InstrumentRouteAuthorisationHandler` (instrument-named base, `instrumentId` parameter) — naming mismatch.
- MCP endpoints require authentication but no scope, despite advertising `api.read`; all five tools are read-only and user-scoped today, but the contract for future tools is implicit convention.
- Claims cache (5 min) means un-sharing an account leaves access live briefly; token validation also performs DB writes (auto-provisioning).

**Recommendations (ordered):**
1. **Fix `GetTagTrendReport`** — add the family filter. This is a bug, not a debate.
2. **Decide the ownership rule and write it down.** A workable contract consistent with what most of the code already does: *route-scoped resources → parameterized policy; family-scoped aggregates → handler/repository filter; commands that accept a foreign key (GroupId, FamilyId) → `ISecurity.Assert*`*. Then delete the layer that's dead under that rule (e.g. the never-failing Tag asserts, or the repository filter — one, not both).
3. **Consider EF global query filters** (`HasQueryFilter`) for `FamilyId` and `Deleted` on family-scoped entities (Tag at minimum). This converts the per-call convention into an architectural guarantee and is precisely the mechanism that would have prevented the tag-trend gap and the thrice-repeated `!t.Deleted && t.FamilyId == user.FamilyId` in Tag queries. Needs a design pass (the current-user accessor must be injectable into the context; `IgnoreQueryFilters` for admin paths).
4. **Unify denial auditing** — the natural home is the handler base classes (or ASM), so every requirement denial is logged, not just budget lines. Fold `AssertInstrumentViewer` into `ISecurity` so there is one wrapper, with one audit behaviour.
5. Pick one truth source per requirement (claims or DB) or document the cache-window divergence as accepted.
6. Delete `GetOwnedInstrumentIds`; unregister the `FamilyMember` named policy (keep the requirement for resource-based use).

---

## Theme 3 — Names that no longer mean what they say

- **`[AggregateRoot]` as a DI switch.** `BudgetLine` (`BudgetLine.cs:5`), `TagRelationship`, and `ImporterType` are attributed solely to get `IQueryable<T>` injection via `AddAggregateRoots<MooBankContext>` — they are not roots. Recommendation: split the concerns — a separate mechanism (or attribute, e.g. `[Queryable]`) for read-side registration, restoring `[AggregateRoot]` as a boundary statement. Natural ASM change.
- **"Repository" covering read-model gateways.** `IReportRepository` (returns DTOs, no aggregate, SP-backed), `SecurityRepository : IAuthorisationRepository`, and `ReferenceDataRepository` (four unrelated entity types, mixed sync/async Add) are query services. Renaming (`IReportReader` / report read service) would let the "repositories = aggregate roots" rule stand without exceptions — half the apparent CQRS violations in the Reports module dissolve with the rename.
- **`Delete` means three things**: `InstrumentRepository.Delete` soft-closes (ClosedDate), `TagRepository.Delete` sets a flag, `GroupRepository.Delete` hard-deletes, `BudgetRepository.Delete` removes an unverified stub. Worth distinct verbs (`Close`, `Archive`, `Remove`) at the repository interface level.
- **`RepositoryDeleteBase.Delete(TKey)` is a runtime trap**: it throws `NotImplementedException` and Tag/LogicalAccount/Rule repositories never override it, so `IDeletableRepository.Delete(id)` compiles and detonates.
- **Child-entity repositories against the aggregate rule**: `Rule` and `RecurringTransaction` are children of the `Instrument` aggregate yet have their own repositories; RecurringTransaction is *also* reachable via specification through the root — two competing access paths. Either promote them to roots deliberately or route access through `IInstrumentRepository`.

---

## Theme 4 — One pattern, two (or three) implementations

**Backend:**
- `.Apply()` (local, ~13 sites) vs `.Specify()` (ASM, 9 files) for applying specifications. Pick one — presumably ASM's, deleting the local extension, or promote `.Apply()` into ASM and delete `.Specify()` usage.
- Specification naming: `IncludeSpecification` vs `XxxDetailsSpecification` vs verb-named `GetWithMembers`/`GetWithCards` for the same "include bundle" concept. `Asset/IncludeSpecification.cs` and `StockHolding/IncludeSpecification.cs` are line-for-line identical.
- Domain events raised in three places: inside aggregates (`Transaction.Create`, `Instrument.AddVirtualInstrument` — the rule's pattern), inside repositories (`InstrumentRepository.cs:11,18` and four siblings appending `InstrumentCreated/UpdatedEvent`), and inside command handlers (`VirtualInstruments/Update.cs:45`, `UpdateBalance.cs:39` — `BalanceAdjustmentEvent`, where a `VirtualInstrument.AdjustBalance()` method would hold the invariant). Meanwhile the same business rule ("balance change ⇒ adjustment transaction") is implemented via event in one flow and via direct transaction creation in `Transactions/Commands/UpdateBalance.cs` in another.
- Three command-binding styles: `[AsParameters]`, attribute-annotated records, and hand-written `BindAsync` with manual JSON deserialization (`VirtualInstruments/Update.cs:18-27`, `UpdateBalance.cs:17-26`). The BindAsync pair is live (CommandBinding.None) but re-implements what the other styles get free.
- `IAuditingUnitOfWork` covers an arbitrary 7 of ~60 commands (Accounts Create/Update, Budgets Create, Families Create, Tags Create/Delete, Transactions Create) — siblings like Tags Update or Families Update save unaudited. Define the audit-worthy set (probably: all user-initiated data mutations) and apply it mechanically.
- Command handlers injecting `IQueryable<T>`: `Budgets/Commands/Generate.cs:22-29` (five IQueryables + inline Includes — the largest deviation), `Tags/Commands/AddSubTag.cs:10`. And `Forecast/Commands/RunForecast.cs` is a pure computation with no writes modelled as a command. Either refactor to the rule or carve out documented exceptions ("complex generation commands may use read-side IQueryables").
- Entity configuration split three ways with duplication: attributes on entities (103 occurrences/43 files), 21 `IEntityTypeConfiguration` classes, and inline `OnModelCreating` — with `UseTptMappingStrategy` and `UseSqlOutputClause(false)` each set in two places for the same entities, and `TransactionInstrumentConfiguration` setting the same column name twice.

**Frontend:**
- Mutation lifecycle: 37 hooks invalidate via `onSettled` + generated keys; tag hooks do manual `setQueryData` cache surgery (`useUpdateTag` does both — the write is immediately redundant); transactions use the centralised partial-key idiom (`transactionKeys.ts`) with optimistic updates. Two hooks invalidate nothing (`useRunRules.ts`, `useUpdateFamily.ts`) — audit whether intentional. Codify: generated mutation + `onSettled` invalidate (+ optimistic layer where UX demands), `toast.promise` for feedback (currently 25 of 57 toast, no rule).
- Form init: 15 `defaultValues`, 1 `values`, and `AccountForm.tsx:45-55` uses a `useEffect`/`setValue` workaround for async data — the exact case `values:` exists for (and it uses `window.alert` for validation).
- Hook placement: tag hooks are split across `src/hooks/`, `routes/tags/-hooks/`, and `routes/accounts/-hooks/`. The implied rule (global = cross-feature) exists; apply it.
- Currency formatting: `Amount.tsx`/`currency.ts` exist but 10 files roll their own `Intl.NumberFormat` and 10 more use `toFixed()`. Dates are clean (date-fns only), though `utils/dateFns.ts` exports a `formatDate` that name-collides with date-fns' own.
- Page shells: `components/AccountPage.tsx`, `assets/-components/AssetPage.tsx`, `shares/-components/StockHoldingPage.tsx` are near-identical wrappers (AssetPage still declares `AccountPageProps`).

---

## Theme 5 — Layering and the undefined core

- **`src/MooBank` (core) is an unnamed grab-bag layer**: audit abstractions, DI helpers, one base command, importer interfaces, in-memory queues, security abstractions, DTO mapping extensions, application services, *and* four `BackgroundService` hosted workers — requiring a `FrameworkReference` to ASP.NET Core from a "core" library. Splitting abstractions from hosted services (or renaming to `MooBank.Application` and moving the workers to the API/jobs host) would give the layer a definition.
- **Domain → Models dependency inversion**: `MooBank.Domain` references the DTO assembly because shared enums (`AccountType`, `TransactionType`, `ScheduleFrequency`, …) live there. This is the root cause of the confusing two-assembly `Asm.MooBank.Models` namespace split (pure DTOs in `MooBank.Models`; Domain-mapping DTOs forced into `src/MooBank/Models/` because MooBank.Models can't reference Domain). **Extracting the shared enums** (into Domain, or a tiny shared-kernel package) fixes both the inversion and the split.
- **Core → concrete integration clients**: `MooBank.csproj` references `MooBank.Abs`/`Eodhd`/`ExchangeRateApi` directly. Conventional layering puts the client interfaces in core and wires implementations in the composition root (`MooBank.Api`).
- **Importers → Infrastructure**: all three `MooBank.Institution.*` projects compile against `MooBankContext`. Pragmatic (their EF configs join the shared model via the static `RegisterAssembly` list — itself order-sensitive global state that silently ignores late registration), but "pluggable" importers are compile-time coupled to the data layer.
- **API is also the job host** — WebJobs classes live in `MooBank.Api/Jobs/`, contradicting "API entry point (minimal, delegates to modules)". Fine as a deliberate consolidation; document it (or restore a jobs host).
- The `//HACK: To be fixed` at `IServiceCollectionExtensions.cs:50` (dual context registration under `IReadOnlyDbContext` with divergent options; injected `IQueryable<T>` actually resolves from the *writable* context) remains the known read/write-split debt — per house convention, the fix is to deliver the capability in ASM, not to work around it locally.

---

## Theme 6 — Dead and vestigial surface (safe deletions)

- `MooBank.Modules.Tags.csproj` → project reference to `MooBank.Modules.Transactions`: the only module→module reference, and **unused** (zero occurrences of "Transaction" in the Tags module).
- `IAuthorisationRepository.GetOwnedInstrumentIds` — zero callers.
- `TransactionAddedEvent` — raised, never handled (decide: implement the documented balance-update handler, or remove the event and the doc example).
- `MooBank.Domain.csproj`: `InternalsVisibleTo` to nonexistent `Asm.MooBank.Modules.Tests`; `Compile Remove` globs for folders that no longer exist.
- Empty/orphaned: `src/Asm.MooBank.Web.App/` (empty leftover), `tests/MooBank.Web.Api.Tests/` (bin/obj only), `tools/MooBank.Tools.*` (not in the solution, reference nothing), `<Folder Include="Endpoints\" />` in Tags csproj.
- Frontend: `AppStore: any` in `configureStore.ts` (self-inflicted, not router-related); `localStorage` write inside the `setPageSize` reducer (`store/Transactions.ts:30` — impure reducer); `store/StockTransactions.ts` is a copy-paste of `store/Transactions.ts` with silently divergent filter logic.

---

## Additional observations (no action urged, worth knowing)

- **Redux is near-vestigial**: 2 slices, both transaction-list UI state, 14 consumer files. Router search params would model this natively (shareable/bookmarkable filters — a real UX gain), decouple mutation hooks from Redux (they currently reach into it to reconstruct query keys), and let `@reduxjs/toolkit`/`react-redux` be dropped.
- **Report SPs are N+1 at the SP layer**: the multi-account variants (`ReportRepository.cs:29-69`) loop per account issuing sequential `EXEC`s. A TVP (e.g. `dbo.GuidList`) or set-based SP is the natural fix for group reports. (No TVP currently exists in the codebase.)
- **SQL project drift exists**: `ExchangeRate.Rate` is `[Precision(12,4)]` in EF vs `DECIMAL(10,4)` in `dbo/Tables/ExchangeRates.sql:6`. With state-based DACPAC deployment and no migrations, periodic spot-checks (or a schema-compare CI step) are the guard.
- **`dbo.AccountBalance` scalar-UDF computed column** re-sums the account's full transaction history per row read — growth-sensitive; also the reason `UseSqlOutputClause(false)` must be remembered on new tables (a latent save-time failure if omitted).
- **`ImportTransactionsService.Import` swallows all exceptions** (logs only) while `RunRulesService` rethrows — import failures are invisible to callers.
- **Error handling above the widget level is thin** (frontend): 0 `errorComponent` usages in routes, no app-level error boundary in app code; `defaultErrorComponent` on the router is the one-line hardening symmetric to the existing `defaultPendingComponent`.
- **Domain richness is bimodal**: `Transaction` is genuinely rich (354 lines, factories, invariants, events); `Budget`/`Group`/`Tag`/`BudgetLine` are property bags mutated in handlers. Pragmatic for CRUD — but tag-hierarchy invariants (self-reference, circularity, same-family, currently in `AddSubTagHandler`) are exactly what the rules say belongs on the entity.
- `Security.AssertAdministrator` accepts a `CancellationToken` it never uses.
- Frontend positives worth preserving as documented conventions: query-key consistency between prefetch/persistence/consumers, the restrained persistence allowlist, clean `-` prefixed co-location (one grey area: `routes/-dashboard/` imports from other features' private `-components`/`-hooks` dirs — either promote those to shared or document the dashboard as a privileged aggregator).

---

## Prioritised recommendations

**P1 — Correctness/security (small, do first)**
1. Add the family filter to `GetTagTrendReport` (only true isolation gap found).
2. Audit the two frontend mutation hooks that invalidate nothing (`useRunRules`, `useUpdateFamily`).
3. Fix the impure `setPageSize` reducer (move the localStorage write to a listener/middleware).

**P2 — Documentation sync (one PR, prevents regression by future agents)**
4. Rewrite `AGENTS.md`; correct CLAUDE.md/PRD layout and router references; mark the services→hooks tech-debt item done; amend the rules docs where the code's pattern is the settled one (event handlers in Domain, inline Includes in queries, commands returning DTOs, Transactions Viewer-for-writes decision).

**P3 — Decide-and-unify (each is a small focused PR)**
5. Tenancy ownership contract + delete the dead duplicate layer (Tags asserts vs repository filter); unify 404-vs-403 idiom.
6. Unify denial auditing in the handler bases (likely an ASM change); fold `AssertInstrumentViewer` into `ISecurity`.
7. One specification idiom (`.Apply()` vs `.Specify()`); one mutation-invalidation idiom + toast rule on the frontend; `values:` for async-seeded forms.
8. Extract shared enums out of `MooBank.Models` to fix the Domain→DTO inversion and the two-assembly namespace split.

**P4 — Structural (larger, schedule deliberately)**
9. EF global query filters for family/soft-delete scoping.
10. Define the core layer: split `src/MooBank` abstractions from hosted services; invert the integration-client dependencies.
11. Separate `[AggregateRoot]` from IQueryable registration (ASM); rename read-model "repositories"; resolve child-entity repositories (Rule, RecurringTransaction).
12. Replace Redux transaction-list state with router search params; drop Redux.
13. Set-based/TVP report SPs for multi-account reports; revisit `dbo.AccountBalance` UDF as data grows.

**P5 — Hygiene sweep (one cleanup PR)**
14. Theme 6 deletions: dead project reference, dead API surface, empty folders, orphaned tools, stale csproj entries, `FamilyMember` named-policy registration.
