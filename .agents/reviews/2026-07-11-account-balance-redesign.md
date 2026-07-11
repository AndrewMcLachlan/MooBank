# Design Spike — `dbo.AccountBalance` redesign (Theme 7.3)

**Status:** **Option B selected by the user and implemented** (2026-07-11). Build + full test suite + SQL project all green. The one item that automated tests do **not** cover — TPT + `ToView` runtime SQL against real SQL Server — remains for the dev-DB validation gate in §7.
**Date:** 2026-07-11
**Related:** `.claude/plans/joyful-tickling-parrot.md` (Theme 7.3), architecture review `2026-07-10-architecture-review.md`.

---

## 1. Problem

`dbo.TransactionInstrument.Balance` is a **non-persisted computed column** backed by a scalar UDF that re-sums the account's entire transaction history on every read:

```sql
-- dbo.Functions/AccountBalance.sql
CREATE FUNCTION [dbo].[AccountBalance] (@AccountId UNIQUEIDENTIFIER NULL)
RETURNS DECIMAL(12,4)
AS BEGIN
    RETURN ISNULL((SELECT SUM(CASE WHEN TransactionTypeId = 1 THEN Amount ELSE -ABS(Amount) END)
                   FROM [Transaction] WHERE AccountId = @AccountId), 0)
END

-- dbo.Tables/TransactionInstrument.sql
[Balance]         AS dbo.AccountBalance([InstrumentId]),
[LastTransaction] AS dbo.LastTransaction([InstrumentId]),   -- sibling: MAX(TransactionTime) per account
```

Two problems compound:

1. **Scalar-UDF-in-computed-column is the classic anti-pattern.** A non-inlined scalar UDF executes **once per row, serially**, with per-invocation overhead, and inhibits parallelism in the calling query. Every place that materialises accounts (dashboard, account lists, instrument details — see §3) pays N invocations for N accounts.
2. **Each invocation re-sums full history.** Cost grows linearly with an account's lifetime transaction count.

`LastTransaction` (`MAX(TransactionTime)` per account) shares the identical pattern and should be redesigned in lockstep.

---

## 2. Key finding — the "zero-change win" is not available as-is

The plan's Option (a) was: *"verify/enable SQL scalar-UDF inlining (Azure SQL supports it — possibly zero-change win)."*

**This does not apply to the current design.** Microsoft's inlineability rules ([Scalar UDF inlining — requirements](https://learn.microsoft.com/sql/relational-databases/user-defined-functions/scalar-udf-inlining#inlineable-scalar-udf-requirements)) state, verbatim, execution-context requirement #8:

> **You don't use the UDF in a computed column or a check constraint definition.**

`AccountBalance`/`LastTransaction` are used **exclusively** through computed columns, so Froid inlining is disqualified regardless of database compatibility level. Confirming `is_inlineable = 1` on the function (it likely is — single `RETURN`, single `SELECT`, no time-dependent/side-effecting intrinsics) is **necessary but not sufficient**: the computed-column usage blocks it.

**Corollary:** inlining only becomes a lever if the aggregation is moved **out of the computed column and into a query/view** — which is exactly what Options B and C below do. So "enable inlining" is not a standalone option; it is a *benefit that Options B/C unlock*.

---

## 3. Current consumption (app-layer surface a change must preserve)

- Domain: `MooBank.Domain/Entities/Instrument/TransactionInstrument.cs` — `[DatabaseGenerated(Computed)] public decimal Balance { get; set; }` (mapped by convention; no explicit EF config).
- DTO mapping reads `account.Balance` directly when building `Instrument.CurrentBalance`:
  - `Modules.Accounts/Models/Account/{LogicalAccount,VirtualInstrument}.cs`
  - `Modules.Instruments/Models/Instruments/{InstitutionAccount,VirtualInstrument}.cs`
- `Instrument.CurrentBalanceLocalCurrency` is derived from `Balance` via `currencyConverter.Convert(...)`.
- Balance is also consumed in Reports (`ReportReader`, monthly-balance reports), Forecast (`ForecastEngine`, starting balance), and import end-balance reconciliation.

Whatever replaces the computed column **must keep a readable `decimal Balance` on the `TransactionInstrument` entity** so these call sites are untouched, or it becomes a large blast-radius change.

### The balance invariant (critical enabler)

Balance is **only ever** `SUM(signed Amount)` over the account's transactions. Manual "set the balance" actions do **not** write a stored balance — they insert an **adjustment transaction** (`Modules.Transactions/Commands/UpdateBalance.cs`, `Modules.Instruments/Commands/VirtualInstruments/UpdateBalance.cs`, raising `BalanceAdjustmentEvent`). Therefore:

- Any maintained/persisted alternative is **provably reconcilable** against `SUM(Transaction)` at any time.
- There is no second writer to reconcile with — the house rule on "never change schemes that persisted data was written under" is satisfiable because the source of truth (the transactions) is unchanged; we only change *how the sum is surfaced*.

---

## 4. Options

### Option A — Enable scalar UDF inlining, keep computed column
**Rejected.** Disqualified by requirement #8 (§2). No behavioural or performance change. Documented only to close it out.

### Option B — Set-based view / inline TVF, computed at read time (no stored aggregate)
Replace both scalar UDFs and both computed columns with one **set-based** aggregate the optimiser can fold into the calling query:

```sql
CREATE VIEW dbo.InstrumentBalance AS
SELECT  AccountId AS InstrumentId,
        SUM(CASE WHEN TransactionTypeId = 1 THEN Amount ELSE -ABS(Amount) END) AS Balance,
        MAX(TransactionTime) AS LastTransaction
FROM    dbo.[Transaction]
GROUP BY AccountId;
```

- **Reads:** a list of N accounts becomes **one grouped scan** (stream aggregate over the existing clustered index `CX_Transaction_AccountId_TransactionTime`) instead of N serial UDF calls. Single-account reads are one range scan (unchanged cost, but no UDF overhead and now parallelisable/inlineable).
- **Writes:** **zero** — nothing stored, no maintenance, no consistency risk.
- **Correctness:** identical by construction (same expression).
- **EF mapping:** map a keyless companion entity (or `ToView`) `InstrumentBalance { InstrumentId, Balance, LastTransaction }` and surface it on `TransactionInstrument` — see §5.
- **Downside:** still recomputes on every read. For this app's scale (personal finance: thousands–tens-of-thousands of rows per account) a clustered-index range-sum is sub-millisecond; the win is eliminating the per-row-UDF pattern, not avoiding the sum.

### Option C — Indexed (materialised) view, engine-maintained aggregate
Same view as B, but **materialised**:

```sql
CREATE VIEW dbo.InstrumentBalance WITH SCHEMABINDING AS
SELECT  AccountId AS InstrumentId,
        SUM(CASE WHEN TransactionTypeId = 1 THEN Amount ELSE -ABS(Amount) END) AS Balance,
        COUNT_BIG(*) AS TxnCount        -- required for indexed views with SUM
FROM    dbo.[Transaction]
GROUP BY AccountId;
GO
CREATE UNIQUE CLUSTERED INDEX IX_InstrumentBalance ON dbo.InstrumentBalance(InstrumentId);
```

- **Reads:** O(1) per account — a seek into the materialised aggregate. Fastest option.
- **Writes:** SQL Server maintains the aggregate **automatically** on every `Transaction` INSERT/UPDATE/DELETE — **no app write-path code, no consistency risk** (engine-guaranteed).
- **Correctness:** engine-maintained; correct with existing + new data with no migration of behaviour.
- **Constraints & downsides:**
  - `MAX(TransactionTime)` **cannot** live in an indexed view (only `SUM`/`COUNT_BIG` aggregates are allowed). `LastTransaction` must stay a cheap non-materialised expression or a separate mechanism.
  - **Write hotspot:** every transaction for a given account updates that account's single aggregate row. Bulk CSV imports (the primary write path) would serialise/contend on per-account aggregate-row maintenance, and every DML pays view-maintenance cost. This is the material risk to weigh against the read win.
  - Requires `SCHEMABINDING` and the usual indexed-view session settings.

### Option D — Persisted running balance maintained by domain logic
Store `Balance` on `TransactionInstrument`, updated incrementally by domain behaviour when transactions are added/updated/removed.

- **Reads:** O(1), no join.
- **Writes:** application-maintained → **highest complexity and highest consistency risk** (must handle every mutation path: import, manual add, edit-amount, delete, split changes, offsets). Any missed path silently corrupts the balance.
- **House-rule exposure:** persisted-scheme rule applies most sharply here. Would require a reconciliation/repair job and a backfill proven identical to `SUM(Transaction)`.
- **Verdict:** most effort, most risk. Only justified if B **and** C are both inadequate at scale — not expected for this app.

### Not viable
- **Persisted computed column** (`AS ... PERSISTED`): disallowed — a persisted computed column must be deterministic and may not reference other tables; `AccountBalance` references `Transaction`.

---

## 5. EF Core mapping considerations (applies to B and C)

The entity must keep a readable `decimal Balance` (and ideally `LastTransaction`) with the call sites in §3 unchanged. Candidate approaches, to be prototyped:

1. **Keyless companion entity mapped to the view**, auto-included and projected onto `TransactionInstrument` — cleanest separation; verify EF emits a single joined query (no N+1) for account-list reads.
2. **`ToView` / table-splitting** so `TransactionInstrument` reads `Balance`/`LastTransaction` from a view while its writable columns remain on the base table — keeps `account.Balance` working verbatim; confirm EF treats them as store-generated/read-only.
3. Keep `LastTransaction` as a trivial correlated expression (or leave its current computed column) if Option C is chosen, since `MAX` can't be materialised.

The chosen approach must produce **one** query for the dashboard/account-list path (join, not per-row), which is the whole point.

---

## 6. Recommendation

1. **Reject A** (inlining is blocked by the computed-column usage — §2).
2. **Adopt Option B (set-based view) as the default.** It removes the anti-pattern, unlocks set-based/inlineable execution, carries **zero write cost and zero consistency risk**, and is correct by construction. For MooBank's data volumes it is almost certainly sufficient.
3. **Hold Option C (indexed view) as the escalation** if, after benchmarking, single-account or aggregate read latency on large accounts is unacceptable — accepting its write-path maintenance cost and per-account hotspot on the import path, and keeping `LastTransaction` non-materialised.
4. **Defer Option D** unless B and C are both proven inadequate; its consistency risk is disproportionate for a derived value the DB can compute.

Effectively: **do the cheap, safe structural fix (B) first; measure; only take on maintained-aggregate complexity (C) if the numbers demand it.**

---

## 7. Validation plan (before/after, whichever option is chosen)

- **Correctness gate (blocking):** on a restored production-shape dev DB, assert
  `SELECT InstrumentId, Balance` from the new mechanism equals the current `dbo.AccountBalance(InstrumentId)` for **every** instrument (full anti-join must return zero rows). Same for `LastTransaction`.
- **Query-shape check:** capture the actual/estimated plan for the dashboard account-list query before and after — confirm the plan no longer contains a `<UserDefinedFunction>` node and does a single grouped join rather than per-row UDF calls.
- **Timing:** record cold + warm execution time for (a) single-account load, (b) full account-list/dashboard load, (c) a representative CSV import (write-path regression check — especially important if Option C is chosen).
- **DACPAC:** verify the database project deploys cleanly (view + index objects; dropped functions/computed columns) with no data loss.
- **Regression:** full backend build + test suite; manual smoke of dashboard, account details, reports (monthly balances), forecast starting balance, and an import end-balance reconciliation.

---

## 8. Decision checkpoint

**Awaiting user decision before any implementation:**

- Proceed with **Option B** now (recommended), or go straight to **Option C**?
- Is the import-path write cost of Option C a concern worth pre-empting (favouring B), or is read latency the priority (favouring C)?
- Confirm the EF mapping approach preference (§5) — or leave it to a prototype spike.

On sign-off, this becomes an implementation task (SQL objects + EF mapping + validation), shipped as its own PR per the standing batching rule.
