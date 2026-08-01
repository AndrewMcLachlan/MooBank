# Design — Forecast update (issue #928)

**Date:** 2026-08-01
**Issue:** [#928 Forecast Update](https://github.com/AndrewMcLachlan/MooBank/issues/928)
**Status:** Approved design, pending spec review → implementation.

## Goal

Three related defects, all the same disease — a single flat number standing in for something that
varies:

1. **Planned expenses that materialise as real transactions throw out the expense calculations**,
   even though they were expected.
2. **Income is modelled twice** — a fixed monthly figure *and* planned income items — so it can
   neither be reconciled nor made to change over time.
3. **There is no single "monthly expenses" number, and the engine pretends there is.** Expenses
   move with income; when extra income ends they must fall back.

All three land together. They touch the same engine, the same result model and the same tests, and
(2) is a prerequisite for (3) — see *Why the offset is the villain* below.

## Decisions (from brainstorming)

- **Matching:** tag on the planned item + a date window. Not explicit transaction links.
- **Income:** remove the fixed monthly field entirely; planned income items are the only model.
  Existing plans are migrated so their figures don't move.
- **Delivery:** one PR, all three parts.
- **Expenses:** income-correlated becomes *the* model, not a mode. The flat average survives only
  as the degenerate case when there is no signal to fit.

## Verified diagnosis

Measured against the production database on 2026-08-01, not inferred. Two plans exist, both with
`"mode":"IncomeCorrelated"` already selected. Ten planned items, **none tagged**. The "Home and
School" plan carries a fixed income of $12,960/month *and* a planned income item ("Margo Work",
$2,536.45/month) — the double-modelling is live, not hypothetical.

### The R² of 0.691 is an artefact

The regression window is `latestTransactionDate − LookbackMonths` (`ForecastEngine.cs:394-395`), so
for the Home and School plan it opens on 30 June 2025 and picks up a **one-day stub month**: income
$0, expenses $9. That single point sits at the origin and anchors the line.

| Training set | Fixed | Slope | R² |
|---|---|---|---|
| 13 months **including** the 30-June stub (what the engine fits) | $2,399 | 0.529 | **0.691** |
| Same window, stub dropped | $6,965 | 0.327 | 0.284 |

Both stub months in the "My Forecast" plan do the same thing in the other direction — with them the
slope is *negative* (−0.054, R² 0.004) and the fit is rejected outright.

### Why the offset is the villain

`regressionIncomeOffset` (`ForecastEngine.cs:96`) reconciles the regression's training income (all
credits) against the plan's single salary figure. With the numbers above:

```
AvgHistoricalIncome  19,997   (inflated by extra income present in history)
planBaseIncome       12,960   (the plan's flat monthly figure)
offset                7,037

every month:  expenses = 2,399 + 0.529 × (12,960 + 7,037) = 12,976
              income                                      = 12,960
                                                            ─────
                                                             −16 / month, flat, to 2033
```

The plan **spends at the high-income level and earns at the low-income level**, in every month.
And it can never recover in 2027 when the extra income ends, because `monthIncome` is the same
constant in every month — the expense line is flat by construction, whatever the slope is.

**Fixing the stub alone makes this worse**, which is why the parts cannot be split: the honest fit
($6,965 + 0.327 × income) through the same offset gives $13,504 against $12,960 income — a
**−$544/month** deficit instead of −$16.

### What the corrected model produces

Fit with the stub dropped, driven by a real per-month income series (no offset):

| Modelled income | Expenses | Net |
|---|---|---|
| $12,960 base only (post-2027) | $11,208 | +$1,752 |
| ~$20,000 with extra income | $13,512 | +$6,488 |
| ~$24,000 recent actual | $14,822 | +$9,178 |

Validation: the actual pre-pay-rise average was **income $12,923, expenses $11,227**. The model
driven at that income predicts **$11,196** — within $31, having never been shown those months as a
target. It reproduces the earlier expense level on its own, which is the required behaviour.

---

## Part A — planned items are *realised*

One organising rule:

> **Baseline outgoings are the spend not covered by a planned item.** A transaction carrying a
> planned item's tag is that item's spend, and is never baseline.

An item **without a tag behaves exactly as it does today** — planned allocation in every month, no
realisation, no baseline subtraction. Realisation is opt-in per item.

### Attribution

`W = OutgoingStrategy.MatchWindowMonths` (new, default 1). Each tagged included item gets a claim
window in whole months:

| Date mode | Claim window |
|---|---|
| `FixedDate d` | `month(d) − W … month(d) + W` |
| `Schedule` | `month(anchor) … month(scheduleEnd ?? planEnd) + W` |
| `FlexibleWindow` | `month(windowStart) − W … month(windowEnd) + W` |

Claim windows are **not** clamped to the plan — a recurring item anchored before plan start must be
able to claim its own history out of the lookback average, or it is counted twice (once in the
baseline, once as a planned item).

Actual spend carrying that tag, in a month inside the window, is attributed to the item. **Shared
tags split proportionally**: for month `M` and tag `T`, claimants split `actual(T, M)` in proportion
to their planned allocation in `M`; where every claimant's allocation in `M` is zero (payment
arrived outside its allocation month but inside the claim window), in proportion to their total
planned amounts. This case is real — Felix and Xander school fees would share one tag.

### Realisation

For item `I` and month `M`, with `latestMonth` = month of the latest transaction:

```
M <= latestMonth        → attributed(I, M)                    // what actually happened
M >  latestMonth, Schedule
                        → plannedAllocation(I, M)             // recurring: can't be "used up"
M >  latestMonth, FixedDate | FlexibleWindow
                        → remaining, re-spread across I's remaining allocation months
                          in the same proportions as planned, where
                          remaining = max(0, I.Amount − Σ attributed(I, ·))
```

This covers all four failure modes from the issue: bill came in at $220 not $200; bill paid a month
late; renovation spread over five months; item planned but never happened.

**Deliberate edge case:** a one-off whose date has passed with nothing matched contributes **0** and
is reported as unrealised, rather than being carried forward. Carrying it forward would be a guess
about intent; the UI flags it and the user moves the date.

### What this fixes downstream

- `RecalculateBaselineFromActuals` — **no change needed.** Once past-month planned figures equal
  the actuals, its `opening + income + planned − closing` algebra cancels exactly. The bug was
  never in that function; it was in the figures fed to it.
- `CalculateBaselineOutgoings` (pre-plan lookback average) — subtract attributed expense in the
  lookback months, then divide.
- `FitIncomeExpenseRegression` — subtract attributed **expense** from each training month.
  Attributed *income* is **not** subtracted: the regression's X is total income, because total
  income is what the forecast feeds it. Consistency between fit and application matters more than
  purity here.

### Reading actuals

A new `IPlannedItemMatcher` in `Services/`, backed by `IQueryable<Transaction>` — following the
Budgets precedent (`Modules.Budgets/Queries/GetValueForTag.cs`), **not** a new stored procedure.
`Transaction` is an aggregate root so `IQueryable<Transaction>` is already registered; no DI change.

Returns `(AccountId, Month, TagId, NetAmount)` over one range covering both the lookback and the
plan. Excludes `ExcludeFromReporting`; nets offsets via the `TransactionSplitNetAmount` DB function;
sums at **split** level so a planned item forming part of a larger transaction is counted correctly.

Rows are aggregated two ways from the one query: over **all plan accounts** for item progress (a car
paid from savings still realises the item), and over the **historical-analysis accounts** (savings
excluded) for the baseline and regression subtractions, matching the account set those figures are
computed over.

### Deleted

`ExpandRealizedNonBaselineExpenses`, `AdjustBaselineForRealizedExpenses` and `IsBaselineFrequency`.
The first two were an earlier attempt at this: `ExpandRealizedNonBaselineExpenses` collects items
dated *on or after* plan start, `AdjustBaselineForRealizedExpenses` keeps only months *before* plan
start, so they can only ever intersect in the plan's own start month, and only when the plan doesn't
start on the 1st. No tests cover them.

---

## Part B — income from planned items only

`IncomeStrategy` leaves the engine and the API entirely: `ManualRecurring`, `ManualAdjustments` and
`HistoricalIncomeSettings`, along with `CalculateHistoricalIncome` and `CalculateIncomeByMonth`.

`IncomeTotal` becomes the month's planned income, realised by the same mechanism when the item is
tagged — so the projected-vs-actual income chart lines up for the first time.

**Explicit deletion, not an oversight:** `HistoricalIncomeSettings.IncludeTagIds` / `ExcludeTagIds` /
`ExcludeTransfers` / `ExcludeOffsets` and `ManualRecurringIncome.Frequency` all carry
"not yet honoured" TODOs, i.e. they are planned functionality. They exist only to serve a
historical-income strategy that this change removes, so they go with it. Nothing else reads them.

### Result model

`ForecastMonth` loses `PlannedItemsTotal` and `PlannedIncomeTotal` — with income no longer
double-modelled they are redundant. The month becomes:

```
ClosingBalance = OpeningBalance + IncomeTotal − BaselineOutgoingsTotal − PlannedExpensesTotal
```

Gained: `RealisedExpensesTotal` (actual attributed spend this month). New on `ForecastResult`:
`PlannedItemProgress[] { PlannedItemId, PlannedTotal, ActualToDate, Remaining, IsUnrealised }`.

### Migration

**Pre-deployment** script — the pattern used for the retirement member move — so it can read the
column before the schema catches up, then the `IncomeStrategy` column is dropped from the table
definition.

For each plan with a non-zero `$.manualRecurring.amount` and no existing planned income item:
insert a `ForecastPlannedItem` (`ItemType = Income`, name `"Income"`, `DateMode = Schedule`) plus a
`PlannedItemSchedule` (`Monthly`, interval 1, anchor = `manualRecurring.startDate ?? plan.StartDate`,
end = `manualRecurring.endDate`). `frequency` is read as Monthly regardless of its value, exactly
matching current engine behaviour, which never honoured it.

Guarded on `COL_LENGTH('dbo.ForecastPlan','IncomeStrategy') IS NOT NULL` so it self-disables once the
column is gone, and on the absence of an income item so it is idempotent.

Neither existing plan has `manualAdjustments` or start/end dates, so both convert to a single
item — $12,960 and $8,192.92 respectively, and the figures don't move. The script still handles
`manualAdjustments` correctly (cumulative deltas become a sequence of items with start/end dates)
rather than assuming the production shape.

---

## Part C — expenses as a function of income

### Whole-month training window

The single highest-value fix, and independent of everything else. A month enters the training set
only when the account data covers all of it (`latestTransactionDate >= last day of that month`).
Partial months at either end are dropped, never fitted.

### The offset survives, but self-diagnoses

Removing it outright would be wrong: the regression trains on all credits (refunds, interest,
transfers in) while planned income items model salary, so a bare comparison understates expenses.
Instead:

```
offset = mean(historical credits)  −  mean(modelled income)   // over the same training months
```

Model income properly and it falls to ~0 on its own. When it doesn't, it is *reporting a real gap in
the income model* rather than silently inflating expenses — surfaced as
`ModelledIncomeShortfall` so the outlook can say so. Where the training window predates the plan,
the offset is computed over the overlapping months only; with no overlap it is 0.

### Guards

`slope >= 0` stays; **`slope <= 1` is added** — spending more than every marginal dollar isn't a
forecast, it's a countdown. `RSquaredThreshold` is **left at 0.5**: it was only ever suspect on the
strength of a number computed against the wrong training set, and once the stub months are gone the
fit stands or falls on its merits. It is a one-line change if it proves too strict.

`LookbackMonths` default 12 → 24 for **new** plans; existing plans keep their stored 12 and can
change it in settings.

### Mode removed

`OutgoingStrategy.Mode` goes. `"HistoricalAverageByTag"` never did anything by tag — it averaged all
debits — so the name was a fiction. The flat average remains as the fallback when the fit is
rejected, reported honestly rather than chosen.

`MatchWindowMonths` is added to `OutgoingStrategy`. Existing stored JSON lacks it and gets the
default of 1; unknown properties are skipped on deserialise, so the retired `mode` needs no
migration. `OutgoingStrategySerialized` is retained.

### Summary model

`RegressionDiagnostics` is promoted from buried diagnostic to the answer — the point being that
there is no single expenses number to report:

```csharp
ExpenseModel {
    decimal FixedComponent;             // $ per month
    decimal VariableComponent;          // fraction of each income dollar
    decimal RSquared;
    int     DataPoints;
    bool    UsingFlatAverage;           // fit rejected
    decimal FlatAverage;
    decimal ModelledIncomeShortfall;    // the offset; 0 is healthy
}
```

---

## Frontend

- **Settings modal** — remove *Monthly Income* and the *Expense Calculation* radio; add
  *Match planned items within ± N months* and *Lookback (months)*.
- **Outlook** — `ForecastOutlook.tsx:22-38` already computes exactly the right thing and hides it in
  a hover popover behind a single `monthlyBaselineOutgoings` figure (line 76). *This is where the
  0.691 is being read from.* Promote it: the fixed and variable components become the headline —
  "about $7,000 a month, plus 33c of every dollar earned" — and the single number goes. Warn when
  `UsingFlatAverage`, and when `ModelledIncomeShortfall` is material ("your modelled income is
  $X/month below the credits actually seen").
- **Income chart** — `ForecastIncomeExpenseCharts.tsx:26` plots `incomeTotal + plannedIncomeTotal`,
  which is the double-modelling made visible: for the Home and School plan that is
  $12,960 + $2,536 against actual credits averaging ~$20,000. It collapses to `incomeTotal`.
- **Planned items table** — Tag picker (`ComboBox`), plus *Spent* and *Remaining* for tagged items,
  and an unrealised marker. Planned Income becomes the only place income is entered, with an
  empty-state prompt.
- While in `PlannedItemsTable.tsx`, replace the `text-muted` utility classes on lines 203 and 222 —
  utility classes are not defined in this app (see `MooBank.Web.App/CLAUDE.md`).

Regenerate the client (`npm run generate`) after the backend builds.

## Testing

Engine (`Modules.Forecast.Tests`): realisation on time and exact, late, over-amount, spread across
months, never happened, untagged item unchanged, shared tag split proportionally. Baseline and
regression subtraction of attributed spend.

**Regression test for the artefact:** a one-day stub month must not enter the training set. This is
the specific defect that produced an R² of 0.691 from nine dollars.

Offset: zero when modelled income matches credits; equals the gap when it doesn't; zero when the
training window and plan don't overlap.

Migration: both production plans convert to a single income item at unchanged amounts; the script is
idempotent and no-ops once the column is dropped.

Frontend (Vitest): settings modal no longer offers income or mode; planned items table renders tag
and progress columns. `ForecastOutlook.test.tsx` and `forecastChart.test.ts` both assert against the
removed fields (`monthlyBaselineOutgoings`, `regression`, `plannedItemsTotal`, `plannedIncomeTotal`)
and move to the new expense model.

## Deliberately out of scope

- **Seasonality.** Decembers and Februarys run $3–6k above trend in the real data — a genuine
  effect, but a separate model. Folding it in would make three changes impossible to judge apart.
  `SeasonalitySettings` stays as its unhonoured stub.
- **Inflation** (`Assumptions.*`) — untouched, still unhonoured.
- **`OutgoingStrategy.ExcludeTagIds` / `ExcludeAboveAmount`** — untouched.
- **Explicit transaction↔item links** — considered and rejected in brainstorming.
- **`FlexibleWindow` in the UI.** The domain, database and API support it fully; only the planned
  items table can't create one, so "build work spread over months" can only be *planned* as a fixed
  date today. Realisation still handles the spread payment via the match window, so the workaround
  is to widen `MatchWindowMonths`. Worth its own small issue — flagged for a decision rather than
  quietly bundled.

## Risks

- **The fit may not survive de-stubbing.** Without the artificial origin point the honest R² is
  0.284 against a threshold of 0.5, so the model may fall back to a flat average until the planned
  items are tagged and the noise is stripped out of the training data. Mitigated by `UsingFlatAverage`
  being visible rather than silent — but it means the improvement may only fully appear once tags
  are applied. This is the main thing to check against real data after the change.
- **Tag specificity.** A broad tag ("Car") will swallow ordinary spending into a planned item and
  suppress the baseline. The window bounds the damage; the per-item Spent figure makes it visible.
