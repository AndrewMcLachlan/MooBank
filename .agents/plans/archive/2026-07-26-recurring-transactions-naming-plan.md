# Recurring Transactions — Naming Consistency & API Consolidation

Closes [#499](https://github.com/AndrewMcLachlan/MooBank/issues/499) (tech-debt, priority 1).

## Decisions

Issue #499 asks three questions. The answers this plan implements:

1. **Should the APIs be for account, virtual account, or both?**
   Virtual instrument only. A recurring transaction is owned by a virtual instrument
   (`VirtualInstrument.RecurringTransactions`, `AddRecurringTransaction`,
   `RemoveRecurringTransaction`), so that is its natural URL. The account-level group is deleted.

2. **Should recurring transactions be returned with the virtual account or separately?**
   Separately — which is already the de facto behaviour. The embedding model
   (`Modules.Accounts.Models.Account.VirtualAccount`) is dead code and is deleted. They are an
   independently mutable child collection with their own query key; embedding them would make
   every recurring-transaction edit invalidate the instrument payload.

3. **Do we differentiate between a virtual (transaction) account and a virtual instrument?**
   No. There is one concept and the naming has drifted. **"Virtual instrument" wins**: it is a
   `TransactionInstrument` subtype, the route is `/virtual` under instruments, and the table is
   already `[dbo].[VirtualInstrument]` — only the FK column kept the old name.

## Global Constraints

- The SPA is the **only** consumer of these endpoints. The MooBank MCP server exposes
  instruments/transactions/tags/reports only — no recurring-transaction tools. A breaking API
  change is therefore safe and needs no deprecation window.
- `src/api/**` and `routeTree.gen.ts` are generated. Never hand-edit; run `npm run generate`
  after the backend builds.
- Build must stay at **0 warnings** (`TreatWarningsAsErrors`).
- Use `String.` for static string methods, not `string.`.
- No SCREAMING_SNAKE_CASE in the frontend; no utility CSS classes.
- Do **not** use `replace_all` for the `VirtualAccount` → `VirtualInstrument` renames —
  `VirtualAccountId` is a substring of nothing, but `VirtualAccount` is a substring of
  `VirtualAccountId` and `VirtualAccountSpecification`. Rename longest-first, or edit by site.

---

## Part A — Naming

### Task 1: Domain — one name, one home

`Domain/Entities/Account/VirtualInstrument.cs` is an instrument sitting in the `Account`
namespace, and its child `RecurringTransaction` refers to it as "VirtualAccount".

**Move** (namespace `Asm.MooBank.Domain.Entities.Account` → `Asm.MooBank.Domain.Entities.Instrument`):

- `Domain/Entities/Account/VirtualInstrument.cs` → `Domain/Entities/Instrument/`
- `Domain/Entities/Account/RecurringTransaction.cs` → `Domain/Entities/Instrument/`
- `Domain/Entities/Account/Events/VirtualInstrumentAddedEvent.cs` → `Domain/Entities/Instrument/Events/`

Leave the rest of `Entities/Account/` alone — `LogicalAccount`, `InstitutionAccount`, `RuleTag`,
`AccountTagPurpose`, `ILogicalAccountRepository` are genuinely account concepts.

**Rename within `RecurringTransaction`:**

| Before | After |
|---|---|
| `Guid VirtualAccountId` | `Guid VirtualInstrumentId` |
| `virtual VirtualInstrument VirtualAccount` | `virtual VirtualInstrument VirtualInstrument` |

`VirtualInstrument.AddRecurringTransaction` sets `VirtualAccountId = Id` — update to match.

**Blast radius:** ~100 files reference `Entities.Account`, but almost all are `using` lines for
`LogicalAccount`/`InstitutionAccount` and are unaffected. Only files touching `VirtualInstrument`,
`RecurringTransaction`, or `VirtualInstrumentAddedEvent` need a using added. Compile-driven —
let the build find them.

**Risk:** if this task's churn proves unpalatable in review, the namespace move can be dropped
independently; the property renames are the part that matters for #499.

### Task 2: Collapse the duplicate specifications

Three specifications have **byte-identical** bodies
(`Include(VirtualInstruments).ThenInclude(RecurringTransactions)`):

- `Domain/Entities/Instrument/Specifications/RecurringTransactionSpecification.cs`
- `Domain/Entities/Instrument/Specifications/VirtualInstrumentSpecification.cs`
- `Domain/Entities/Account/Specifications/VirtualAccountSpecification.cs`

Keep **`VirtualInstrumentSpecification`** (it describes what is loaded — the virtual instruments
with their recurring transactions). Delete the other two and repoint callers:

| Caller | Currently uses |
|---|---|
| `MooBank/Services/RecurringTransactions.cs` | `RecurringTransactionSpecification` |
| `Modules.Accounts/Commands/Recurring/Create.cs` | `RecurringTransactionSpecification` |
| `Modules.Accounts/Commands/Recurring/Update.cs` | `RecurringTransactionSpecification` |
| `Modules.Accounts/Commands/Recurring/Delete.cs` | `RecurringTransactionSpecification` |
| `Modules.Instruments/Commands/VirtualInstruments/Update.cs` | `VirtualAccountSpecification` |
| `Modules.Instruments/Commands/VirtualInstruments/UpdateBalance.cs` | `VirtualAccountSpecification` |

Delete the two now-redundant test files:
`tests/MooBank.Core.Tests/Specifications/RecurringTransactionSpecificationTests.cs` and
`VirtualAccountSpecificationTests.cs`. Fold any assertion they make that
`VirtualInstrumentSpecificationTests.cs` does not already cover into the surviving file.

### Task 3: Rename `Queries/VirtualAccounts` → `Queries/VirtualInstruments`

`Modules.Instruments/Queries/VirtualAccounts/` contains `Get.cs` and `GetForAccount.cs` returning
`VirtualInstrument`. Rename the folder and namespace
(`…Queries.VirtualAccounts` → `…Queries.VirtualInstruments`).

While there: `GetForAccount` is the only query in the module named for "account" rather than
"instrument". Rename to `GetForInstrument` and align its `AccountId` parameter with
`InstrumentId` to match the route (`/instruments/{instrumentId}/virtual`).

### Task 4: Database — rename the column

`[dbo].[RecurringTransaction].[VirtualAccountId]` → `[VirtualInstrumentId]`, and
`FK_RecurringTransaction_VirtualAccount` → `FK_RecurringTransaction_VirtualInstrument`.

The rename **must** be accompanied by a refactorlog entry. Without one SqlPackage sees
*drop column + add column* and destroys every recurring transaction's parent link on publish.

`MooBank.Database.refactorlog` is currently an empty `<Operations/>` element — it was cleared
deliberately once the historical entries became clutter, not because renames are unsupported.
Add a `Rename Refactor` operation for the column so the publish emits `sp_rename`.

Verify by generating a deployment script against a restored production copy and confirming
`sp_rename`, **not** `ALTER TABLE … DROP COLUMN`.

---

## Part B — API consolidation

> Tasks 5–7 are the breaking part. They can ship as a second PR if Part A is worth landing early.

### Task 5: One endpoint group

**Delete** `Modules.Accounts/Endpoints/Recurring.cs` and
`Modules.Accounts/Endpoints/VirtualRecurring.cs`.

**Add** `Modules.Instruments/Endpoints/VirtualInstrumentRecurring.cs`:

```
Path => "instruments/{instrumentId}/virtual/{virtualInstrumentId}/recurring"
Tag  => "Recurring Transactions"
```

| Verb | Route | Query/Command |
|---|---|---|
| GET | `/` | `GetAll` |
| GET | `/{recurringTransactionId}` | `Get` |
| POST | `/` | `Create` |
| PATCH | `/{recurringTransactionId}` | `Update` |
| DELETE | `/{recurringTransactionId}` | `Delete` |

The old account-level `GetAll` ("all recurring transactions across every virtual instrument of an
account") is **dropped, not moved** — nothing calls it. If the forecast page later wants it, add
one read at `GET instruments/{instrumentId}/recurring`.

**Move** the CQRS types from `Modules.Accounts` to `Modules.Instruments`, so the module that owns
virtual instruments owns their children:

- `Commands/Recurring/{Create,Update,Delete}.cs`
- `Queries/Recurring/{Get,GetAll,GetForVirtual}.cs` — `GetForVirtual` becomes the new `GetAll`
  (it is already the "for one virtual instrument" query); delete the old account-scoped `GetAll`.
- `Models/Recurring/RecurringTransaction.cs`

Update `Modules.Accounts/Module.cs` (remove both registrations) and
`Modules.Instruments/Module.cs` (add one).

**Every command now takes `VirtualInstrumentId` from the route, not the body.** `Create` currently
smuggles it through the JSON body because the account-level URL has nowhere to put it; that is the
whole reason the frontend hooks are littered with `as any`. Rework:

- `Create(Guid InstrumentId, Guid VirtualInstrumentId, string? Description, decimal Amount, ScheduleFrequency Schedule, DateOnly NextRun)`
- `Update(…, Guid RecurringTransactionId, …)`
- `Delete(Guid InstrumentId, Guid VirtualInstrumentId, Guid RecurringTransactionId)`

Keep the `InstrumentIdCommand` base and `BindAsync` (`BindHelper.BindWithInstrumentIdAsync`),
switching the route-param name from `"accountId"` to `"instrumentId"`. Note `Delete` currently
does **not** derive from `InstrumentIdCommand` — make it consistent with the other two.

`Delete`'s handler currently finds the virtual instrument by searching for one that *contains* the
recurring transaction id. With `virtualInstrumentId` in the route it should look it up directly and
404 if the recurring transaction is not in that instrument's collection.

### Task 6: Authorisation — unchanged

The group registers `Policies.GetInstrumentViewerPolicy` for reads *and* writes. **Viewer is the
house standard across the app, not an oversight here** — leave it alone. The only change is the
route-parameter name the policy binds to: `"accountId"` → `"instrumentId"`.

The handler checks that the virtual instrument belongs to the instrument remain as
defence-in-depth, per the authorisation ownership contract.

### Task 7: Delete the dead `VirtualAccount` model

`Modules.Accounts/Models/Account/VirtualInstrument.cs` defines `record VirtualAccount :
VirtualInstrument` plus a full `VirtualAccountExtensions.ToModel`. Nothing references it — no
endpoint produces it, and it duplicates the live mapping in
`Modules.Instruments/Models/Instruments/VirtualInstrument.cs`. Delete the file.

This is the concrete answer to #499's second question: recurring transactions are returned
separately, and the only code that said otherwise was unreachable.

---

## Part C — Frontend

### Task 8: Regenerate and rewire

1. `dotnet build MooBank.slnx` (regenerates `openapi-v1.json`), then
   `npm run generate` in `src/MooBank.Web.App`.
   Kill the running `Asm.MooBank.Api` process first if it holds a file lock.
2. `src/models/recurringTransactions.ts` — `emptyRecurringTransaction(virtualAccountId)` →
   `virtualInstrumentId`, and drop the field from the body entirely if the generated create model
   no longer carries it (it moves to the route).
   **Also fix `Schedules`** — see "Flagged" below.
3. The four hooks in `routes/accounts/-hooks/`:
   `useGetRecurringTransactions`, `useCreateRecurringTransaction`,
   `useUpdateRecurringTransaction`, `useDeleteRecurringTransaction`.
   All four now use one operation family with `path: { instrumentId, virtualInstrumentId }`.
   **The `as any` casts must go** — they exist only because the write path's shape did not match
   the read path's. If a cast is still needed after regeneration, the backend binding is wrong;
   fix it there rather than casting.
   Keep the existing optimistic `setQueryData` + rollback in all three mutations — do not
   downgrade them to invalidate-only.
4. `routes/accounts/-components/RecurringTransactions.tsx` — rename local `accountId`/`virtualId`
   to `instrumentId`/`virtualInstrumentId` and update the props type.

The frontend *route* URLs (`/accounts/$id/virtual/$virtualId/…`) are user-facing and unchanged;
this task only touches API calls.

---

## Task 9: Tests

**Update (move with their subjects, `Modules.Accounts.Tests` → `Modules.Instruments.Tests`):**
- `Commands/Recurring/{CreateTests,UpdateTests,DeleteTests}.cs`
- `Queries/Recurring/{GetTests,GetAllTests,GetForVirtualTests}.cs` — `GetAllTests` for the deleted
  account-scoped query goes; `GetForVirtualTests` becomes the `GetAll` tests.
- `Support/TestEntities.cs` in every affected test project (namespace/property renames).

**Delete:** `Core.Tests/Specifications/{RecurringTransactionSpecificationTests,VirtualAccountSpecificationTests}.cs`.

**`Api.Tests/Authorization/InstrumentAuthorizationTests.cs`** — three tests hit the old routes
(lines ~361, ~385, ~426). Update the URLs to the new instrument-scoped paths. Authorisation
behaviour is unchanged, so the assertions stand as they are.

**New:** a test asserting `Delete` 404s when the recurring transaction exists but belongs to a
*different* virtual instrument than the one in the route — the new route makes this reachable.

---

## Validation

```bash
dotnet build MooBank.slnx          # 0 warnings, 0 errors
dotnet test tests/                 # all green
cd src/MooBank.Web.App
npm run generate                   # then confirm no unstaged hand-edits under src/api/
npx tsc --version                  # must report 7.x
npm run build
npm run lint
npm test
```

Manual: open a virtual account's Manage page, then create / edit / delete a recurring transaction
and confirm the optimistic update and the toast both behave.

Database: generate a publish script against a restored production copy and confirm `sp_rename`
(see Task 4). Do not skip this.

---

## Found while reading — flagged, not in this plan

1. **`Yearly` and `Fortnightly` schedules crash the background job.**
   `ScheduleFrequency` has `Daily=1, Weekly=2, Monthly=3, Yearly=4, Fortnightly=5`, but
   `RecurringTransactionService.Process` switches on Daily/Weekly/Monthly only and throws
   `InvalidOperationException` otherwise. The throw is inside the `foreach` with no per-item
   catch, so **one Yearly recurring transaction stops every other user's recurring transactions
   from running**. The frontend `Schedules` array offers `"Yearly"` — so this is reachable from
   the UI today. Worth its own issue; a one-line fix in Task 8 step 2 (removing `Yearly` from the
   picker) would only paper over it.

2. **`LastRun` is offset-wrong off-UTC.** Domain is `DateTime?` over `DATETIME2`; the model is
   `DateTimeOffset?` and relies on the implicit conversion. EF reads the column back as
   `Kind=Unspecified`, so the conversion stamps the *server's local* offset — but the service
   writes `DateTime.UtcNow`. Harmless on Azure (UTC), wrong by the local offset in development.
   Relatedly `RunTransaction` uses `DateTime.Now` for the transaction time while `LastRun` uses
   `DateTime.UtcNow`.

Both are behavioural, not naming; keeping them out keeps this PR reviewable.
