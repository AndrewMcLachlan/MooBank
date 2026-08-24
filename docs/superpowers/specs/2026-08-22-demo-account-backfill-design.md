# Demo account backfill

Issue [#924](https://github.com/AndrewMcLachlan/MooBank/issues/924), part one.

Brings the Demo family's accounts up to date and fills the ones that were never populated, so the
dashboard and reports are worth showing. The recurring monthly job the issue also asks for is
deliberately **not** in this spec: a job that keeps data current is worth little while the mortgage
is ten years stale, so the data comes right first and the job follows in its own spec.

## Where the demo family actually stands

Family `B0DDD93D-827F-4716-B4E2-D1922FAF7E27` ("Demo"), measured against the local copy of
production on 2026-08-22.

| Account | Type | Rows | Range |
|---|---|---|---|
| Checking Account | Transaction | 9,323 | 2014-01-01 → **2026-05-09**, 9,228 tagged (99%) |
| Savings Account | Savings | 247 | 2014-01-01 → **2025-11-30** |
| Mortgage | Mortgage | **1** | `Opening Balance −243,550.54`, 2016-05-09 |
| Super | Superannuation | **1** | `Opening Balance 153,244.00`, 2016-05-09 |
| Credit Card | Credit | **1** | `Opening Balance −5,320.16`, 2020-01-01 |
| Demo Account | Transaction | **0** | — |
| House | Asset | — | — |

Three facts drive the whole design:

1. **Mortgage, super and credit card are not stale, they are empty.** Each holds an opening balance
   and nothing else. The dashboard shows a mortgage that has not been paid down since 2016.
2. **No `AccountTagPurpose` rows exist for any demo account.** Principal vs Interest, Super
   Contributions, Super Returns and Savings Interest classify transactions through that table, so
   they are blank no matter what transactions exist.
3. **The demo family owns no utilities accounts.** The electricity and water accounts in the
   database belong to the McLachlan family.

## The principle: derive, do not invent

The checking account already holds the other side of almost everything, regularly and cleanly:

| Tag | Count | Range | Average |
|---|---|---|---|
| Mortgage | 148 | 2014-01-28 → 2026-04-28 | 2,200.00 (flat) |
| Salary | 156 | 2014-02-01 → 2026-05-01 | 6,686.32 |
| Electricity | 104 | 2014-01-12 → 2026-04-06 | 269.32 |
| Water | 52 | 2014-01-04 → 2026-03-22 | 218.78 |
| Rates | 52 | 2014-01-04 → 2026-03-30 | 450.16 |

So each new account is built **as a function of those rows** — same dates, same amounts — rather
than from an independently invented schedule. The mortgage ledger cannot disagree with the bank
account, because it is derived from it. Only super and the car loan are generated independently,
and for stated reasons.

**Rates need no work at all.** They are already quarterly tagged transactions on the checking
account with no utilities account behind them, which is exactly the treatment agreed for them.

## Scripts

One idempotent script per piece under `src/MooBank.Database/Scripts/`, following the convention
already there (`BEGIN TRAN` … `COMMIT`, set-based, no cursors). They are **not** referenced by the
`.sqlproj` and are not part of any deployment: they are run by hand against production, once, and
each is guarded to be a no-op on a second run.

Every script targets the Demo family by id and asserts the expected account exists before writing.
A script that cannot find its target raises an error rather than writing nothing silently.

### 1. `DemoTagPurposes.sql`

Inserts `AccountTagPurpose` rows so the four purpose-driven reports have something to classify:

| Account | Purpose | Tag |
|---|---|---|
| Mortgage | `MortgageInterest` (4) | new `Mortgage Interest` |
| Super | `EmployerContribution` (2) | new `Employer Contribution` |
| Super | `PersonalContribution` (3) | new `Personal Contribution` |
| Savings Account | `Interest` (1) | existing interest tag if present, else new `Interest` |

Creates the tags in the Demo family where they do not exist. Guard: skip any pair already present.

Smallest script, and a prerequisite for 2 and 3 being visible anywhere.

### 2. `DemoMortgage.sql`

Re-dates the opening balance to 2014-01-01, immediately before the first repayment, and sets it so
that amortising the 148 known repayments produces a plausible balance today.

**The ledger runs positive-owing**, which is the opposite sign to the balance the account holds
today, and the direction is forced rather than chosen. Principal vs Interest derives principal as
(monthly debit total − interest-tagged splits) on the mortgage account, so the whole repayment has
to be a debit there; the balance view subtracts every debit, so the month's interest has to be
credited back or the balance runs away from zero instead of toward it. No code negates a balance by
account type, so the mortgage will read as a positive balance on the dashboard.

Proposed figures, and the main thing to review in this script:

- Principal **387,500** at **5.5%** nominal annual, monthly rest — the standard 30-year loan that a
  2,200 monthly repayment services.
- Amortised over the 148 actual repayment dates, this leaves roughly **298,000** owing today, which
  reads as a 30-year loan twelve years in.

For each of the 148 checking `Mortgage` transactions it writes one mortgage-account transaction on
the same date, split into interest and principal for that period, with the interest split tagged
`Mortgage Interest`. Principal vs Interest then covers the full history the checking account shows.

Guard: no-op if the mortgage account already has more than its opening balance row.

### 3. `DemoSuper.sql`

Super never touches the bank account, so this is generated rather than derived — but from the salary
that is already there.

- **Employer contributions** on each `Salary` date from 2016-05-09 onward — around 116 of the 156,
  since the opening balance postdates the first salary — at the superannuation guarantee rate in
  force for that date, tagged `Employer Contribution`. Earlier salary dates are skipped rather than
  producing a balance that contradicts the opening figure.
- **Earnings** quarterly at a nominal **7%** a year with deterministic scatter of roughly ±3
  percentage points, so the balance chart rises with visible wobble rather than as a straight line.
  Scatter is seeded from the date, so re-running produces identical figures.
- Opening balance stays at 2016-05-09.

No personal contributions are generated. The tag purpose is configured so the report renders the
series, and an empty personal series against a populated employer one is honest for this household.

### 4. `DemoUtilities.sql`

Creates two utilities accounts for the Demo family — **Electricity** (`UtilityType` 1) and **Water**
(3) — and one bill per existing checking payment:

- 104 electricity bills, cost matching the tagged payment, one period each, with a supply charge and
  a consumption usage row whose rate and quantity multiply out to the remainder.
- 52 water bills, quarterly, with water and sewerage service charges and one consumption row.

Bills are shaped so `utilities.TotalCost` reproduces the amount actually paid from checking. Meter
readings run monotonically upward across bills so the usage reports have a sensible series.

Depends on the charge types seeded by the current schema (`Supply`, `Water Service`,
`Sewerage Service`).

It also depends on a **fix to `utilities.TotalCost`**, included in this branch. The version on main
joins service charges to usages, so a water bill carrying two service charges counts its
consumption twice and the bill totals wrongly. The fix sums the two sets separately. It is the same
change already sitting on the feed-in branch, lifted byte-identical so the two merge without
conflict; it does not depend on anything else there.

### 5. `DemoCarLoan.sql`

The only piece with no existing side, so it writes to the checking account as well. Bounded
deliberately:

- A **five-year loan of 35,000 at 7.5%**, drawn 2022-07-01, repaying **701.35** monthly.
- 50 repayment transactions inserted into checking to date, tagged `Car Loan` (new tag), plus the
  matching amortising loan account of type `Loan` (7).
- Ends mid-2027, so the demo shows a loan in progress rather than one already closed.

This is the only script that modifies the checking account, which is otherwise accurate and 99%
tagged. It is separate from the others so it can be run, reviewed, or skipped on its own.

### 6. `DemoSavings.sql`

The Savings Account stops at 2025-11-30, nine months behind checking. The recurring job fills one
month — the previous one — and never looks further back, so nothing else will ever close this gap.

Extends savings from 2025-12-01 to the end of the last whole month, deriving transfers in from the
checking account's existing transfers over the same window and accruing interest on the running
balance, the way `SavingsAccountGenerator` does.

Guard: no-op if the account already holds transactions after 2025-11-30.

### Not included

- **Credit Card** keeps its lone opening balance. Populating it means inventing a second spending
  stream that would have to reconcile with checking's existing merchants, for a card that is not
  central to any report.
- **Demo Account** (0 transactions) is left alone. It looks like a stray; deleting someone's account
  is not this spec's business, but it is worth a look.

## Verification

Each script ends with a `SELECT` reporting what it wrote, so the run is checkable at the console
rather than by reasoning about it afterwards.

Beyond that, after all scripts:

| Check | Expectation |
|---|---|
| Mortgage balance today | ≈ −298,000, and monotonically decreasing |
| Interest + principal per row | equals 2,200 for every mortgage row |
| Principal vs Interest report | non-empty over 2014 → today |
| Super balance today | > opening balance, rising with visible variation |
| Super Contributions report | employer series populated |
| Savings Account last transaction | end of the last whole month, matching checking |
| Each generated bill's `Cost` | equals the checking payment it derives from |
| Checking row count | unchanged except by `DemoCarLoan.sql` (+50) |
| Re-running any script | writes nothing |

The scripts are developed against the local copy, which is the same data, so every figure above can
be confirmed before anything is run in production.

## Follow-up, not in this spec

The recurring monthly job, specified separately in
[the monthly job design](2026-08-23-demo-account-monthly-job-design.md). It extends checking,
savings, super, the mortgage, the loan and the bills by one month on the first of each month, and
deliberately does no catch-up — which is why every gap, savings included, is closed here.
