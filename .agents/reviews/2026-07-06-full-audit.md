# MooBank Full Codebase Audit — 2026-07-06

Scope: entire repository (backend + frontend), audited by six parallel review agents covering
Domain/Infrastructure, core CQRS modules, reporting/finance modules, API host/security/importers,
frontend app logic, and frontend components. Read-only audit; no code was changed.

Severity is a triage judgment by the coordinator; confidence is the reviewing agent's.

---

## 1. Security / Authorization

| # | Location | Issue | Confidence |
|---|----------|-------|------------|
| S1 | `src/MooBank.Modules.Transactions/Commands/UpdateTransaction.cs:29`, `AddTag.cs:15`, `RemoveTag.cs:15` | **IDOR**: handlers fetch the transaction by `Id` via an unscoped repository and never verify `transaction.AccountId == request.InstrumentId`. Authorization validates only the route's instrument, so any user can retag / edit splits/notes of any other family's transaction by posting its GUID under their own account route. Fix: assert `entity.AccountId == request.InstrumentId` (404 otherwise) or scope the query. | High |
| S2 | `src/MooBank.Modules.Accounts/Commands/SetTagPurpose.cs:24-26` | `TagId` never validated against the user's family — cross-tenant tag reference possible. Fix: resolve via family-scoped `ITagRepository`. | Medium |
| S3 | `src/MooBank.Modules.Accounts/Module.cs:17-19`, `src/MooBank.Modules.Instruments/Module.cs:16-18`, `src/MooBank.Modules.Transactions/Module.cs:15`, `src/MooBank.Modules.Assets/Endpoints/Assets.cs:32`, `src/MooBank.Modules.Stocks/Endpoints/StockHoldings.cs:36` (+ stock transactions group) | Write endpoints (imports, rules, virtual instruments, transaction create/update, asset/stock PATCH/POST) are guarded by `GetInstrumentViewerPolicy` instead of `GetInstrumentOwnerPolicy` — family members with view-only access can mutate shared instruments. Bills (`BillAccounts.cs:30`) shows the intended owner-policy pattern. | Medium (may be deliberate; contradicts PRD pattern) |
| S4 | `src/MooBank.Security/Authorisation/InstrumentOwnerAuthorisationHandler.cs:10` | `value is Guid instrumentId` is always false — route values are strings — so the handler always calls `context.Fail()`. Every endpoint using the route-param owner policy (e.g. `Modules.Bills/Endpoints/BillAccounts.cs:30`) returns 403 for legitimate owners. Fix: `Guid.TryParse(value.ToString(), ...)` like the viewer handler. | High |
| S5 | `src/MooBank.Infrastructure/Repositories/LogicalAccountRepository.cs:25` | The ownership/family security filter in `GetById` is never executed — ASM's `RepositoryBase.Get` doesn't call it and nothing else does. Repository-level tenancy filtering on logical accounts silently doesn't exist (endpoint policies are the only guard). | High |
| S6 | `src/MooBank.Modules.Transactions/McpTools/TransactionTools.cs:18` + `InstrumentViewerAuthorisationHandler` | Resource-based `AuthorizeAsync` on the `/mcp` endpoint shares its requirement type with the route-param handler; with no `instrumentId` route value the route handler calls `Fail()`, vetoing the resource handler — MCP transaction access should always be denied. Fix: separate requirement types or don't `Fail()` on absent route value. | Medium |
| S7 | `src/MooBank.Modules.Budgets/Queries/Get.cs:20` | Side-effecting GET: `GET /budget/{year}` creates the budget when missing; two concurrent first-time GETs can insert duplicate `(FamilyId, Year)` rows, after which `SingleOrDefaultAsync` throws 500s permanently. | Medium |

## 2. Backend Correctness Bugs

### Broken endpoints (fail every time)

| # | Location | Issue | Confidence |
|---|----------|-------|------------|
| B1 | `src/MooBank.Modules.Accounts/Commands/Recurring/Update.cs:16` | `BindAsync` reads route key `"instrumentId"` but the route declares `{accountId}` — every PATCH to update a recurring transaction returns 400. Fix: pass `"accountId"` as `Create.cs:14` does. | High |
| B2 | `src/MooBank.Modules.Families/Commands/RemoveMember.cs:35` | `family.AccountHolders` is never loaded (no Include, no lazy loading), so `Count <= 1` is always true — Remove Family Member always throws "Cannot remove the last member". Unit tests mask it via mocks. | High |
| B3 | `src/MooBank.Modules.Institutions/Commands/Update.cs:20` | `(int)RouteValues["id"]!` unboxes a string — `InvalidCastException` → 500 on every `PATCH /institutions/{id}`. | High |
| B4 | `src/MooBank.Modules.Accounts/Queries/InstitutionAccounts/Get.cs:11-13` | No `Include(a => a.InstitutionAccounts)` on a no-tracking queryable — the collection is always empty, endpoint always 404s (it's also the CreatedAtRoute target for create). | High |
| B5 | `src/MooBank.Modules.Instruments/Queries/Rules/Get.cs:11-13` | Same pattern: no `Include(a => a.Rules)` — `GET /instruments/{id}/rules/{ruleId}` always 404s. | High |
| B6 | `src/MooBank.Modules.Tags/Commands/AddSubTag.cs:10,25-26` | Handler injects `IEnumerable<TagRelationship>`; only `IQueryable<>` is registered, so DI supplies an **empty** enumerable — duplicate and circular-relationship checks never fire; tag cycles can be created. Also compares `tr.Tag == subTag` by reference. | High |

### Wrong results / data corruption

| # | Location | Issue | Confidence |
|---|----------|-------|------------|
| B7 | `src/MooBank.Modules.Instruments/Commands/VirtualInstruments/Update.cs:41-48` | Balance reconciliation wrong three ways: only acts when balance decreases; `BalanceAdjustmentEvent` amount sign is inverted vs `UpdateBalance.cs:37`; sets `Balance` directly **and** raises the event (double application). Fix: mirror the `UpdateBalance` handler. | High |
| B8 | `src/MooBank.Modules.Tags/Queries/GetTagsHierarchy.cs:16` | Root filter inverted (`t.TaggedTo.Count != 0` selects tags that *have* parents as roots) — true roots never appear, subtrees repeat per non-root tag. Fix: `== 0`. | Med-High |
| B9 | `src/MooBank.Domain/Entities/Transactions/Specifications/SortSpecification.cs:21-44` | No unique tie-breaker in ordering consumed by paging — transactions repeat on one page and vanish from another. Fix: append `ThenBy(t => t.Id)`. | High |
| B10 | `src/MooBank.Infrastructure/Repositories/InstrumentRepository.cs:35-36` | `?? throw new NotFoundException()` applied to the `Task`, not its result — never fires; missing instrument yields NRE → 500 instead of 404 in many handlers. Fix: await, then null-coalesce. | High |
| B11 | `src/MooBank.Modules.Instruments/Commands/Rules/Update.cs:14` + `RuleRepository.cs:15` | `Get(instrumentId, ruleId)` lacks `Include(r => r.Tags)` — PATCH response always has empty `Tags`. | Med-High |
| B12 | `src/MooBank.Modules.Accounts/Queries/GetAll.cs:16-18` | No Includes before `ToModelAsync` — `GET /accounts` always returns empty `institutionAccounts`/`virtualInstruments`/`tagPurposes` and null `remainingBalance`. | Medium |
| B13 | `src/MooBank.Modules.Users/Commands/Update.cs:26-38` | Cards keyed by `Last4Digits` with `Single(...)` — duplicate last-4 digits → 500. | Medium |
| B14 | `src/MooBank.Domain/Entities/Account/Specifications/AccountDetailsSpecification.cs:7-8` | Spec omits `Owners.User/Group` and `Viewers.User` Includes needed by `GetGroup`/`ValidViewers` — NRE when `ShareWithFamily` is true; `GroupId` silently null in command responses. Same pattern in `StockHolding.ValidAccountViewers`. | Medium |
| B15 | `src/MooBank.Infrastructure/Repositories/TagRepository.cs:26-27` | Bulk `Get(tagIds)` doesn't filter `!t.Deleted` — soft-deleted tags can be re-attached. | Medium |
| B16 | `src/MooBank.Infrastructure/Repositories/ReferenceDataRepository.cs:20` | `AddStockPrice` dupe check ignores `Exchange` — same ticker on two exchanges drops the second price. | Medium |
| B17 | `src/MooBank.Infrastructure/Interceptors/ExistingTagByIdInterceptor.cs:36` | `.Single(...)` over tracked `TagSettings` throws and fails the whole `SaveChanges` when no match. Fix: `SingleOrDefault`. | Medium |
| B18 | `src/MooBank.Infrastructure/EntityConfigurations/Transaction.cs:37-39` | `Extra` JSON conversion has no `ValueComparer` and round-trips as `JsonElement` — in-place mutations never saved; consumers get back a different type. | Medium |
| B19 | `src/MooBank.Domain/Entities/Transactions/Specifications/FilterSpecification.cs:23` | LIKE metacharacters (`%`, `_`, `[`) not escaped in user filter text — wildcard semantics leak (low severity). | High |
| B20 | `src/MooBank.Infrastructure/Repositories/GroupRepository.cs:7-10` | `Delete(Guid)` overridden with an empty body — silent no-op (latent; current callers use `Delete(entity)`). Related: `RepositoryDeleteBase.Delete(TKey)` throws `NotImplementedException`. | High (no-op) |
| B21 | DateTime handling: `BalanceAdjustmentEventHandler.cs:21`, `VirtualInstrumentAddedEventHandler.cs:16` (`DateTime.Now`), `Modules.Transactions/Commands/Create.cs:35` + `UpdateBalance.cs:29` (`TransactionTime.LocalDateTime` = server TZ) | Inconsistent local-vs-UTC stamping shifts transaction times by deployment timezone; date-bucketed reports can place them in the wrong day/month. | Medium-High |
| B22 | `src/MooBank.Modules.Instruments/Commands/VirtualInstruments/UpdateBalance.cs:37-41` | Handler only raises the event; the event handler no-ops for non-`TransactionInstrument` — endpoint returns 200 with an unchanged balance. Fix: validate instrument type up front. | Medium |
| B23 | `src/MooBank.Models/StockSymbol.cs:14,33-42,60` | `TryParse` throws on symbols without a dot; validation regex `\w*` matches everything (no-op); `operator ==` NREs on null left operand. `Parse` reachable via implicit string conversion. | High |
| B24 | `src/MooBank.Models/Quarter.cs:84-85` | `Equals(object)` recurses into itself (StackOverflow for boxed Quarter) / `InvalidCastException` for other types. Fix: `obj is Quarter q && Equals(q)`. | Med-High |

### Reports & financial math

| # | Location | Issue | Confidence |
|---|----------|-------|------------|
| R1 | `src/MooBank.Modules.Budgets/Queries/ReportForMonthBreakdown.cs:54,60` | **Split double-counting**: budget-line actuals sum whole-transaction `Math.Abs(NetAmount)` per tag — a $150 transaction split $100/$50 contributes $150 to *both* lines. Sibling `ReportForMonthBreakdownUnbudgeted.cs:42` shows the correct per-split approach. | High |
| R2 | `src/MooBank.Modules.Reports/Queries/GetByTagReport.cs:30`, `GetUserSpendingByTag.cs:46-53` | Same defect class: per-tag totals use whole-transaction `NetAmount` per tag — split/multi-tag transactions counted in full under every tag. | High |
| R3 | `src/MooBank.Database/dbo/StoredProcedures/GetCreditDebitTotals.sql:24` (also `GetMonthlyCreditDebitTotals.sql`, `GetMonthlyTotalsForTag.sql`) | `t.TransactionTime <= @EndDate` (DATE = midnight) excludes end-date transactions with a time component — month-end manual entries drop out of every SP-backed report. Fix: `< DATEADD(day, 1, @EndDate)`. | High |
| R4 | `src/MooBank.Modules.Reports/Queries/GetTagTrendReport.cs:58-82` | `ApplySmoothing` loses and double-counts amounts across gaps (Jan=10, Apr=30, May=5 → total 65 instead of 45). | High |
| R5 | `src/MooBank.Modules.Reports/Queries/GetPrincipalVsInterestReport.cs:51` | Sign convention mixed between the two data sources — principal = |debits| **+** |interest| instead of −; `InterestTotal` negative and plotted raw. | High |
| R6 | `src/MooBank.Modules.Reports/Queries/GetUserSavingsBreakdown.cs:35-44` | Per-instrument `Delta` always 0 for the default one-month range (first row == last row); longer ranges exclude the first month's movement. | High |
| R7 | `src/MooBank.Modules.Forecast/Services/PlannedItemExpander.cs:96-104` | `Schedule.Interval <= 0` (unvalidated user input) → infinite loop + unbounded list — hangs `RunForecast` (DoS). | High |
| R8 | `src/MooBank.Modules.Forecast/Services/PlannedItemExpander.cs:50-55` | `AllAtEnd` window entirely after the plan is still allocated in full at the final month (EvenlySpread branch is guarded; this one isn't). | Medium |
| R9 | `src/MooBank.Modules.Stocks/Queries/GetStockValueReport.cs:22-25,42` | `StockPriceHistory` not filtered by `Symbol` — with >1 symbol the chart uses an arbitrary stock's price. | High |
| R10 | `src/MooBank.Modules.Bills/Queries/Accounts/GetAllByType.cs:22` | `.Min(b => b.IssueDate)` throws when a group has accounts with zero bills — 500 right after creating a new bill account. | High |
| R11 | `src/MooBank.Modules.Bills/Queries/Reports/GetUsageReport.cs:51` | Days computed exclusively vs the schema's inclusive `DaysInclusive` — usage/day overstated (÷89 vs ÷90). | Medium |
| R12 | `src/MooBank.Modules.Budgets/Endpoints/Budget.cs:37` | Location header hardcodes `year = 2023` (`// HACK`). | High |
| R13 | `src/MooBank.Modules.Forecast/Endpoints/PlannedItems.cs:26` | Location header uses `planId = Guid.Empty` — dead link. | High |
| R14 | `src/MooBank.Modules.Reports/Queries/GetByTagReport.cs:12,45` | `ParentTagId` accepted from the route but never used to filter — "By Tag Report For Tag" returns identical data to the parentless route. | High |

### Importers & external clients

| # | Location | Issue | Confidence |
|---|----------|-------|------------|
| I1 | `src/MooBank.Institution.AustralianSuper/Importers/Importer.cs:103-113` | Validation chains all four `Decimal.TryParse` calls with `&&` after a short-circuiting check — for normal contribution rows **none of the TryParse calls execute**; all contribution amounts persist as 0. | High |
| I2 | `src/MooBank.Institution.AustralianSuper/Importers/Importer.cs:155-156` | `PaymentPeriodEnd`/`Start` likely swapped (`[0]`/`[1]`), and unguarded `DateOnly.ParseExact` aborts the whole import on malformed periods. | Med/High |
| I3 | `src/MooBank.Institution.Ing/Importers/IngImporter.cs:61-75` + AustralianSuper `Importer.cs:61-75` | Hand-rolled CSV quote handling drops commas inside quoted fields and mis-splits fields with 2+ commas. Fix: use CsvHelper (Macquarie already does). | High |
| I4 | `src/MooBank.Institution.Ing/Importers/IngImporter.cs:171`, `MacquarieImporter.cs:168` | `endBalance!.Value` throws when the file is empty / all rows invalid or pending — import crashes instead of returning empty. | High |
| I5 | `src/MooBank.Institution.Macquarie/Importers/MacquarieImporter.cs:104-111` + `TransactionRawRepository.cs:15-16` | Pending-update path uses `FirstAsync` — a genuinely new transaction with identical details/date/amount aborts the entire import. Fix: `FirstOrDefaultAsync` + fall through to insert. | High |
| I6 | `src/MooBank.Institution.Ing/Importers/IngImporter.cs:123-125` | Duplicate detection matches `null == null` receipt numbers — same-day same-amount transactions with unparseable descriptions silently skipped as duplicates. | High |
| I7 | `src/MooBank.Institution.Ing/Importers/TransactionParser.cs:60,74` | Culture-sensitive, unguarded `DateTime.Parse` inside the import loop — aborts the entire import on Azure's invariant culture. Fix: `TryParseExact` with explicit format. | High |
| I8 | Culture-sensitive parsing throughout: `IngImporter.cs:84,101,106,114`, `MacquarieImporter.cs:50,71,76,89`, AustralianSuper `Importer.cs:105-108,115`, `AbsClient.cs:68` | No `CultureInfo.InvariantCulture` on decimal/date parsing — breaks under comma-decimal or invariant host cultures. | Medium |
| I9 | `src/MooBank.Institution.Ing/Importers/TransactionParser.cs:131` | Loose `DirectPayment` regex ordered before more specific variants — location/purchase-date/card digits lost, cardholder attribution never happens for those rows. | Medium |
| I10 | `src/MooBank.Institution.Macquarie/Importers/MacquarieImporter.cs:114-121` | Sequence numbers assigned in reverse chronological order (comment says the opposite) and start at 2 — same-day transactions sort inverted. | High (mismatch) |
| I11 | `src/MooBank.Api/IServiceCollectionExtensions.cs:41-71` | First-login provisioning race (two concurrent requests → PK violation → 500), and `audit.LoginSuccess` fires on **every request**, flooding the audit log. | Medium |
| I12 | `src/MooBank/Services/ImportTransactionsService.cs:42-45` | `Import` swallows all exceptions with only a log — failed imports invisible to the user. | High |
| I13 | `src/MooBank/Services/CurrencyConverter.cs:23` | `.Result` sync-over-async on a hot path (per-account dashboard conversions) — starvation/deadlock risk. | High |

## 3. Frontend Bugs

### React Query cache correctness

| # | Location | Issue | Confidence |
|---|----------|-------|------------|
| F1 | `routes/accounts/-hooks/useUpdateTransaction.ts:41` (+ `useAddTransactionTag`, `useRemoveTransactionTag`, `useCreateTransaction`, `shares/-hooks/useCreateStockTransaction`) | Transaction-list invalidation is a **silent no-op**: the filter key includes `instrumentId: "", pageSize: 0` which never matches a real key. Only window-focus refetch masks this. Fix: invalidate by id-only prefix. | High |
| F2 | `hooks/useCreateTag.ts:23,29` | Tag names double-URL-encoded ("My Tag" → server stores "My%20Tag") — generated client already encodes path params. | High |
| F3 | `routes/accounts/-hooks/useInvalidateSearch.ts:11` | Invalidation keyed on `transaction.id` where the cache uses the account id — search results stay stale after edits. | High |
| F4 | `routes/forecast/-hooks/useRunForecast.ts:13-20` (+ 4 invalidating hooks) | `setQueryData(key, undefined)` is a no-op in TanStack v5 and no query ever reads `["forecast", planId, "result"]` — the whole result-cache scheme is dead. | High |
| F5 | `routes/budget/-hooks/useUpdateBudgetLine.ts:11` | Editing a budget line never invalidates `getBudget({year})` — edited amounts visually revert. | High |
| F6 | `routes/accounts/-hooks/useAddRuleTag.ts:19-21`, `useRemoveRuleTag.ts:20-23` | Optimistic updates mutate the cached array in place and `setQueryData` with the same reference (no re-render, no rollback); `splice(-1)` removes the *last* tag when not found. | High |
| F7 | `settings/families/-hooks/useCreateFamily.ts:20-22`, `settings/institutions/-hooks/useCreateInstitution.ts:18-21` | Same in-place `push` + same-reference `setQueryData`; optimistic entries keep client-side ids forever (no invalidation). | High |
| F8 | `routes/accounts/-hooks/useCreateRecurringTransaction.ts:20` (+ update/delete variants) | `getQueryData` result used without null check — TypeError when the query isn't cached; in-place mutation. | High |
| F9 | `routes/accounts/-hooks/useUpdateVirtualInstrumentBalance.ts:36` | Optimistic update sets `accounts.total = 0` (grand total displays $0 until refetch); nested in-place mutation, no rollback. | High |
| F10 | `routes/accounts/-hooks/useUpdateTransaction.ts:24-38` (+ tag hooks) | Optimistic updates mutate shared cached `Transaction` objects — memoized rows don't re-render; failed mutation leaves cache permanently wrong (`onSettled` invalidation is F1's no-op). | High |
| F11 | `routes/groups/-hooks/useCreateGroup.ts:16-18`, `useUpdateGroup.ts:16-18` | Wrapper doesn't return the `mutateAsync` promise — awaits don't wait; rejections unhandled. | High |
| F12 | `routes/budget/-components/BudgetReport.tsx:46-53` | Mutates the cached `budgetYears` array in place (`push`/`sort` on the cached reference). | High |

### UI correctness

| # | Location | Issue | Confidence |
|---|----------|-------|------------|
| F13 | `routes/budget/-components/BudgetReportTags.tsx:18,53,73` + `-hooks/useBudgetReportForMonthBreakdown.ts:5` + callers | Budget month indexing internally contradictory (hooks send `month+1`, headers render `month-1`, the two callers pass different bases) — year-chart drilldown fetches **one month after** the bar clicked; dashboard link shows the previous month in the header. Standardise the `$year/$month` param as 1-based. | High |
| F14 | `routes/accounts/$id/-rules/RuleRow.tsx:18-24` | Infinite render loop while tags load (`?? []` new array as effect dep that setStates). | High |
| F15 | `models/transactions.ts:12` + `NewTransactionSplit.tsx` + `TransactionSplits.tsx` | All unsaved splits share `emptyGuid` — duplicate React keys; editing/removing the second split edits/removes the first (or both). | High |
| F16 | `routes/accounts/-transactions/details/TransactionSplitTagPanel.tsx:11` | Panel state never re-syncs after save — previous split's tags shown and re-attached to the next new split. | High |
| F17 | `hooks/period.ts:30-35` | `getPeriod()` throws when `period-id` is "-1" with no stored period — breaks 9+ report pages at render. | High |
| F18 | `utils/dateFns.ts:15-23` | "This Year" period inverted every January (end = last Dec 31 < start); all period constants computed once at module load — stale in long-lived tabs. | High |
| F19 | `routes/accounts/-transactions/components/FilterPanel.tsx:17` + `hooks/useFilterPanel.ts:23,26,73-79` | Applied vs displayed type filter disagree after reload; stored tag/type filters written but never read back (`??` fallbacks unreachable); `clear()` doesn't reset local state. | High |
| F20 | `components/Amount.tsx:33` | `(Math.abs(amount) ?? 0)` — `??` doesn't catch NaN; renders "$NaN" while data loads. | High |
| F21 | `components/AccountList/VirtualAccountRow.tsx:49-51` | Effect keyed on whole `props` object — background refetch clobbers the balance mid-typing. | High |
| F22 | `components/TransactionSearch.tsx:18` + `TransactionSplit.tsx:61,72` | Clearing a refund/offset search passes null into handlers that dereference it — TypeError. | Medium |
| F23 | `.../reports/-components/MonthlyBalances.tsx:29,43,47`, `groups/-components/GroupMonthlyBalances.tsx:31` | Charts plot `Math.abs(balance)` (loans chart positive; zero-crossing draws a false V); page/section mislabelled "Tag Trend". | Med/High |
| F24 | `routes/-dashboard/Summary.tsx:13` | `filter(ag => ag.total)` hides groups netting exactly $0 from Net Worth. | High |
| F25 | `routes/accounts/-transactions/components/Import.tsx:18,46-47` | Unguarded `openAccounts.length`/`.map` (crash while loading); `closedDate === null` filter drops accounts when the API omits the property. | Medium |
| F26 | `components/ReportTypeSelector.tsx:6-18` | `...rest` never spread — callers' `hidden` prop silently ignored. | High |
| F27 | `components/PeriodSelector.tsx:46,55-137` | `value` prop ignored — `<MiniPeriodSelector value={period}>` silently uncontrolled. | High |
| F28 | `hooks/useInOutReport.ts:6` + sibling report hooks | `formatISODate(start)` evaluated in the query key before `enabled` — RangeError during render when start/end undefined; `useTagTrendReport` has no enabled guard at all. | Medium |
| F29 | `routes/profile.tsx:31-34` | Card deletion filtered by `last4Digits` — deletes both/wrong card on shared digits. | High |
| F30 | `components/CurrencySelector.tsx:172` | Mojibake: "BolÃ­var Soberano". | High |
| F31 | `api/types.gen.ts:1256,3338` | Generated types declare `path?: never` for endpoints with URL placeholders — backend OpenAPI spec missing path-parameter declarations (root cause of `as any` workarounds). | High |
| F32 | `routes/accounts/-transactions/components/FilterPanel.tsx:18` | `window.location.search` in a dependency array is non-reactive. | Medium |

## 4. Dead Code

### Backend

- `src/MooBank.Domain/Entities/Transactions/TransactionComparer.cs` — zero usages. (High)
- `src/MooBank.Domain/Entities/Transactions/Events/TransactionAddedEvent.cs` — raised on every transaction create; **no handler exists anywhere** — pure dispatch overhead. (High)
- `src/MooBank.Domain/Entities/Tag/Specifications/IncludeInReportingSpecification.cs` — only its own test references it. (High)
- `src/MooBank.Domain/Entities/Account/ImportAccount.cs` + `ILogicalAccountRepository.RemoveImportAccount` — entity not even in the EF model; no callers. (High)
- `src/MooBank.Infrastructure/DbSetExtensions.cs` — internal `FindAsync` never invoked. (High)
- `src/MooBank.Domain/Entities/Group/Group.cs:27-28` — `[NotMapped] Accounts` never read or populated. (High)
- `IInstrumentRepository.Reload` + implementation — never called. (High)
- `InstrumentRepository.GetInstitutionAccount` + private `GetById` — zero callers, not on the interface. (High)
- `GroupRepository.GetById` — no callers. (High)
- `ITagRepository.AddSettings` — called only from tests; its null guard is unreachable anyway. (High)
- `MooBankContext.VirtualAccounts` DbSet — unreferenced (entity discovered via navigation). (Medium — verify)
- `src/MooBank.Security/Authorisation/BudgetLineRequirementHandler.cs` + `Policies.BudgetLine` — handler never registered; requirement never used in src (tests only). Would fail closed if used. (High)
- `src/MooBank.Security/ClaimsUserDataProvider.cs` — never registered; duplicates `SettableUserDataProvider`. (High)
- `GetUnprocessed` in all three institution `TransactionRawRepository` classes + interfaces — zero callers (also has a latent Id-vs-TransactionId bug). (High)
- `AuthorisationExtensions.AssertInstrumentOwner` — no callers. (Medium — public API)
- `src/MooBank/Models/Extensions/IEntityTypeConfiguration.cs` — empty interface shadowing EF Core's; no implementers; name-collision trap. (High)
- `IngImporter.cs:181,202-211` + AustralianSuper `Importer.cs:180` — `unprocessed` computed then never used; adjacent commented-out blocks. (High)
- `ExchangeRateClient.GetExchangeRate` — no callers; also missing the auth header, so it'd fail if wired up. (High)
- `Modules.Budgets/Queries/ReportForMonthBreakdownUnbudgeted.cs:73-99` — `IncludeWhereRelationship` extension: zero references + commented-out block. (High)
- `Modules.Reports/FinancialYear.Range` and `ReportQuery.ExcludeOffset` — tests only. (High — verify)
- `Modules.Forecast/Models/Strategies.cs` — many strategy knobs (`IncludeTagIds`/`ExcludeTagIds`/`ExcludeTransfers`/`ExcludeOffsets`, `OutgoingStrategy.*`, `ManualRecurringIncome.Frequency`, entire `Assumptions` record) are persisted via the API but **never read by the forecast engine** — silently ignored user settings. Implement or remove. (High)
- "By Tag Report For Tag" route (`Modules.Reports/Endpoints/Reports.cs:36`) — never called by the app; handler ignores the parameter anyway. (Medium)
- Three identical unused `CreateCreateHandler` helpers: `Modules.Accounts/Endpoints/Accounts.cs:47-55`, `InstitutionAccounts.cs:36-44`, `Modules.Instruments/Endpoints/Instruments.cs:29-37`. (High)
- Unused module DTOs: `Modules.Accounts/Models/Account/ImportAccount.cs`, `Modules.Instruments/Models/Rules/CreateRule.cs`, `Modules.Families/Models/CreateFamily.cs`. (High)
- Unused `ToEntity` mapping chains in `Modules.Accounts/Models/Account/{LogicalAccount,InstitutionAccount,VirtualInstrument}.cs`. (Medium — verify)
- Dead locals: `GetByTagReport.cs:19-20` (`start`/`end`), `Modules.Budgets/Models/Budget.cs:29,50` (`mask`), `GetTagTrendReport.cs` (`current` + commented-out block).

### Frontend

- `src/extensions.ts` — whole file (`toNameValue`/`NameValue`) unused. (High)
- `src/store/App.ts` — dead Redux slice: registered, never dispatched, never selected. (High)
- `serviceWorkerRegistration.ts` + `service-worker.ts` — gate is always false under Vite (`import.meta.env.NODE_ENV`), would crash on `process.env.PUBLIC_URL`, and no SW asset is emitted; `main.tsx:22` calls it for nothing. (High)
- `routes/shares/-hooks/keys.ts` — neither export imported. (High)
- Dead exports: `utils/queryString.ts` `addOrReplaceQueryString`; `utils/dateFns.ts` `periodEquals`; `utils/tags.ts` `compareTags`/`compareTagArray`; `models/stocks.ts` `emptyStockHolding`; `models/reports.ts` `BaseReport`. (High)
- `components/AccountTypeSelector.tsx` — 0-byte empty file. (High)
- `components/AccountList/ManualAccountRow.tsx` — never imported. (High)
- `TransactionSplitTagPanel.transactionId` prop — declared, passed, never read. (High)
- `routes/accounts/$id/reports/-components/InOut.tsx:18` — bare no-op expression statement. (High)
- `TopTags.tsx:16,29-35` — permanently-false `showGross` branch (has a TODO). (High)
- `NewRule.tsx:42-44,69` — `onCreate` wired while `allowCreate={false}`; handler creates a tag but never attaches it. (Medium)
- `AGENTS.md` in `src/MooBank.Web.App` documents the pre-migration architecture (services/, pages/, React Router) — misleading. (Doc)

## 5. Memory / Performance

### Backend

| # | Location | Issue |
|---|----------|-------|
| P1 | `src/MooBank.Infrastructure/Repositories/ReportRepository.cs:29-69` | The three `…ForAccounts` "batch" methods run **one stored-proc round trip per account, sequentially**. `RunForecast` pays ~4×N SP calls per run; dashboard reports (`GetUserCashFlow`, `GetUserSpendingTrend`, `GetUserSavingsBreakdown`) pay N each. Fix: TVP/set-based SPs. (Flagged independently by two agents.) |
| P2 | `src/MooBank.Infrastructure/Repositories/TransactionRepository.cs:8-15` | `GetTransactions(instrumentId)` loads the account's **entire transaction history, change-tracked, with `Splits.Tags`** — used by all three importers and `RunRulesService` on every import. Fix: date-window + `AsNoTracking` projection for dedup; track only rows being retagged. |
| P3 | `IngImporter.cs:123-124` | Duplicate check runs `ParseDescription` (13 regexes) on every existing raw transaction × every CSV line — O(history × lines) regex work (~2M parses for 10k history × 200 lines). Precompute receipt numbers once. All three importers also load full raw history per import (AustralianSuper materializes full entities incl. `Include(Transaction)` for three fields). |
| P4 | `IngImporter.cs:135` | Per-row `GetByCard` DB query inside the import loop (N+1) — an in-class cache (`GetAccountHolder`) exists but only `Reprocess` uses it. |
| P5 | `src/MooBank/Services/ExchangeRateService.cs:19,27` | `tos.Contains(...)` on an unmaterialized `IQueryable` inside an in-memory `Where` — one SQL query per API rate (~170) per currency, nightly. Materialize first. Jobs also lack `CancellationToken`s. |
| P6 | `src/MooBank.Modules.Reports/Queries/GetSuperReturnsReport.cs:66-71` | `GetMonthlyTotalsForTag` called per financial year × per tag (e.g. 30 SP calls/request). One call over the range, group in memory. |
| P7 | In-memory aggregation on report hot paths: `GetInOutTrendReport.cs:17`, `GetByTagReport.cs:23` (plus a second identical query for untagged), `GetUserSpendingByTag.cs:38-42`, `ReportForMonthBreakdown.cs:46` + `ReportForMonthBreakdownUnbudgeted.cs:36` (full `TagRelationship` closure per request; deferred `otherTagIds` re-evaluated inside `Sum` per transaction — quadratic; gratuitous `AsParallel`), `Modules.Bills/Queries/Bills/GetForAccount.cs:14-17` (loads every bill then pages in memory) | Equivalent SQL aggregation already exists for most (`GetCreditDebitTotals`, `GetTransactionTotalsByTag`). |
| P8 | `src/MooBank.Modules.Instruments/Queries/Instruments/GetFormatted.cs:39-75` | Deferred `groups` enumerable enumerated 2×, each group's `matchingAccounts` enumerated 2× — the dashboard's heaviest query does its in-memory work ~2-4×. `ToList()` both. |
| P9 | `Modules.Accounts/Queries/Recurring/{Get,GetAll,GetForVirtual}.cs`, `Modules.Instruments/Queries/VirtualAccounts/Get.cs` | Load the whole instrument aggregate (all virtual instruments + all recurring transactions) to select one child in memory. |
| P10 | `Modules.Tags/Queries/GetAll.cs:12-18` | 4-level nested `Include(t => t.Tags)` on every `GET /tags` — multiplicative join on one of the most-called endpoints; `ToModel` needs one level. |
| P11 | Sync EF on async paths: `SecurityRepository.AssertGroupPermission` (`Any()`), `ReferenceDataRepository.AddStockPrice` (`Any()`), `BudgetRepository.AddLine` (`Reference().Load()`), `InstrumentRepository.Delete` (`Find`) | Thread-pool blocking under load; convert to async equivalents. |

### Frontend

| # | Location | Issue |
|---|----------|-------|
| P12 | `routes/-dashboard/Forecast.tsx:26-30` | Runs a **server-side forecast computation on every dashboard mount** (result lives only in mutation state, nothing reused). Model "latest result" as a real query with `staleTime` (fixes F4 too). |
| P13 | `routes/accounts/-transactions/Transactions.tsx:71-72` | Desktop and compact `TransactionList` both fully mounted and hidden via CSS — double row rendering, tag panels, and two `TransactionDetails` modals per page. Render one via a media-query hook. |
| P14 | `components/TransactionListProvider.tsx:19-25` | Unmemoized context value — every provider re-render re-renders all transaction rows. |
| P15 | `.../components/TransactionTagPanel.tsx:18-23` | Per-row derived state via effect+setState instead of `useMemo` — extra render pass per visible row on every tag change. |
| P16 | `components/TransactionSearch.tsx:19` | `JSON.stringify(filteredTransactions)` as a `key` every render — stringifies the full result set and remounts the ComboBox (losing focus) on changes. |
| P17 | `utils/dateFns.ts:15-23` | Period constants computed once at module load — stale month boundaries in long-lived tabs. Make them functions. |

Minor: `getStepSize` returns NaN for empty datasets and is copy-pasted in three chart files; array-index keys in `AccountList.tsx:12` / `Summary.tsx:25`; `PrecacheService` `PeriodicTimer` never disposed; EODHD API token passed in URL query string; exchange-rate 12h cache not invalidated by the nightly job; `DateTime.Now` vs `UtcNow` inconsistency in `RecurringTransactionService`.

---

## Suggested triage order

1. **S1 (transaction IDOR)** and **S4 (owner policy always denies)** — one is a cross-tenant write hole, the other a broken auth handler; both are small fixes.
2. **Broken endpoints B1–B6** — user-facing features that fail 100% of the time (recurring-transaction update, remove family member, institution PATCH, two GET-by-id endpoints, tag-cycle protection).
3. **Report correctness R1–R6** — split double-counting and SP end-date exclusion mean the numbers users see are simply wrong.
4. **Importer robustness I1–I7** — silent zeros, aborted imports, and false duplicates directly corrupt imported data.
5. **Frontend cache bugs F1–F12** — a systemic pattern (in-place cache mutation, no-op invalidation) fixable with one shared convention.
6. **P1–P3, P12–P13** — the highest-leverage performance items.
7. Dead code — mechanical cleanup, any time.
